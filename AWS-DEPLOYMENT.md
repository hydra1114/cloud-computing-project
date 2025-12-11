# AWS Deployment Guide (EC2 + Docker)

## Prerequisites

1. AWS Account
2. AWS CLI installed and configured
3. Docker Desktop installed
4. .NET 9.0 SDK
5. Node.js 18+

## Backend Deployment: Docker on EC2

This guide uses Amazon Elastic Container Registry (ECR) to store your Docker image and an EC2 instance to run it.

### Step 1: Create ECR Repository

```powershell
# Create repository
aws ecr create-repository --repository-name inventory-api --region us-east-1

# Authenticate Docker to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

### Step 2: Build and Push Image

```powershell
cd backend

# Build image
docker build -t inventory-api .

# Tag image
docker tag inventory-api:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/inventory-api:latest

# Push to ECR
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/inventory-api:latest
```

### Step 3: Launch EC2 Instance

1. Go to AWS Console > EC2 > Launch Instance.
2. **Name:** `inventory-backend`
3. **AMI:** Amazon Linux 2023 AMI (Free tier eligible).
4. **Instance Type:** `t3.micro` (Free tier eligible).
5. **Key Pair:** Create a new key pair (e.g., `inventory-key.pem`) and download it.
6. **Network Settings:**
   - Allow SSH traffic from "My IP".
   - Allow HTTP traffic from the internet.
   - Allow HTTPS traffic from the internet.
7. **IAM Instance Profile:**
   - Create a new IAM Role for EC2 with `AmazonEC2ContainerRegistryReadOnly` policy attached.
   - Attach this role to the instance in Advanced Details.
8. Launch Instance.

### Step 4: Configure EC2 Instance

Connect to your instance using SSH:

```powershell
ssh -i "path/to/inventory-key.pem" ec2-user@YOUR_EC2_PUBLIC_IP
```

Inside the EC2 instance, run:

```bash
# Update system
sudo dnf update -y

# Install Docker
sudo dnf install docker -y
sudo service docker start
sudo usermod -a -G docker ec2-user

# Enable Docker to start on boot
sudo systemctl enable docker

# Log out and log back in to pick up group changes
exit
```

Reconnect via SSH, then authenticate Docker to ECR:

```bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

### Step 5: Run Application

Run the container on the EC2 instance. Note: You need your RDS connection string here.

```bash
docker run -d \
  -p 80:8080 \
  --name inventory-api \
  --restart always \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=YOUR_RDS_ENDPOINT;Database=inventory;Username=admin;Password=YOUR_PASSWORD" \
  YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/inventory-api:latest
```

*Note: The backend listens on port 8080 inside the container (default for .NET 8+), mapped to port 80 on the host.*

### Step 6: Configure CORS

After deployment, update the CORS policy in your code to include the EC2 Public DNS/IP:

```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "https://your-cloudfront-domain.cloudfront.net",
    "http://YOUR_EC2_PUBLIC_DNS"
)
```

## Frontend Deployment to S3 + CloudFront

### Step 1: Create S3 Bucket

```powershell
# Create bucket
$bucketName = "inventory-frontend-" + (Get-Random)
aws s3 mb s3://$bucketName --region us-east-1

# Enable static website hosting
aws s3 website s3://$bucketName `
  --index-document index.html `
  --error-document index.html
```

### Step 2: Create Bucket Policy

Create a file `bucket-policy.json`:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::YOUR_BUCKET_NAME/*"
    }
  ]
}
```

Apply policy:

```powershell
aws s3api put-bucket-policy `
  --bucket $bucketName `
  --policy file://bucket-policy.json
```

### Step 3: Build and Deploy Frontend

```powershell
cd frontend

# Update API URL in .env
"VITE_API_URL=http://YOUR_EC2_PUBLIC_DNS/api" | Out-File -FilePath .env -Encoding utf8

# Build
npm install
npm run build

# Deploy to S3
aws s3 sync dist/ s3://$bucketName
```

### Step 4: Create CloudFront Distribution

```powershell
# Create distribution
aws cloudfront create-distribution `
  --origin-domain-name "$bucketName.s3.amazonaws.com" `
  --default-root-object index.html
```

### Step 5: Update CORS

Update backend CORS to include CloudFront domain:

```powershell
cd backend
# Edit Program.cs to add CloudFront domain
# Rebuild and push Docker image
# Pull and restart container on EC2
```

## RDS Database Setup

### Create PostgreSQL RDS Instance

```powershell
aws rds create-db-instance `
  --db-instance-identifier inventory-db `
  --db-instance-class db.t3.micro `
  --engine postgres `
  --master-username admin `
  --master-user-password YOUR_PASSWORD `
  --allocated-storage 20 `
  --storage-type gp2 `
  --no-multi-az `
  --vpc-security-group-ids sg-xxxxxxxx `
  --publicly-accessible `
  --port 5432
```

**Important Security Group Configuration:**
1. Go to the Security Group used by your RDS instance.
2. Add an Inbound Rule:
   - **Type:** PostgreSQL
   - **Source:** Custom -> Select the Security Group ID of your EC2 instance (e.g., `sg-yyyyyy`).
   - This allows the EC2 instance to talk to the database securely.

### Run Migrations

You can run migrations from your local machine if the RDS is publicly accessible (restrict IP in SG), or run them from the EC2 instance.

```powershell
cd backend
# Update connection string in appsettings.json temporarily
dotnet ef database update
```

## Continuous Deployment

### GitHub Actions (EC2 Deployment)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to AWS EC2

on:
  push:
    branches: [ main ]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v1
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1

      - name: Build, tag, and push image to ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: inventory-api
          IMAGE_TAG: ${{ github.sha }}
        run: |
          cd backend
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:latest .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest

      - name: Deploy to EC2
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.EC2_HOST }}
          username: ec2-user
          key: ${{ secrets.EC2_SSH_KEY }}
          script: |
            aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin ${{ steps.login-ecr.outputs.registry }}
            docker pull ${{ steps.login-ecr.outputs.registry }}/inventory-api:latest
            docker stop inventory-api || true
            docker rm inventory-api || true
            docker run -d -p 80:8080 --name inventory-api --restart always \
              -e ASPNETCORE_ENVIRONMENT=Production \
              -e ConnectionStrings__DefaultConnection="${{ secrets.DB_CONNECTION_STRING }}" \
              ${{ steps.login-ecr.outputs.registry }}/inventory-api:latest

  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup Node
        uses: actions/setup-node@v2
        with:
          node-version: '18'
      
      - name: Build
        run: |
          cd frontend
          npm ci
          npm run build
      
      - name: Deploy to S3
        run: |
          aws s3 sync frontend/dist/ s3://${{ secrets.S3_BUCKET }} --delete
          aws cloudfront create-invalidation --distribution-id ${{ secrets.CF_DISTRIBUTION_ID }} --paths "/*"
        env:
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          AWS_DEFAULT_REGION: us-east-1
```

## Monitoring and Logging

### CloudWatch Logs

To send Docker logs to CloudWatch, you need to configure the `awslogs` log driver on the EC2 instance.

1. Attach `CloudWatchAgentServerPolicy` to your EC2 IAM Role.
2. Run container with logging options:

```bash
docker run -d \
  --log-driver=awslogs \
  --log-opt awslogs-region=us-east-1 \
  --log-opt awslogs-group=/ec2/inventory-api \
  --log-opt awslogs-create-group=true \
  ... (other options)
```

## Cost Optimization

- **EC2 Instance:** `t3.micro` is Free Tier eligible (750 hours/month).
- **RDS:** `db.t3.micro` is Free Tier eligible.
- **Stop Instances:** Stop your EC2 and RDS instances when not in use to save credits/money.
  - EC2: `aws ec2 stop-instances --instance-ids i-xxxxxx`
  - RDS: `aws rds stop-db-instance --db-instance-identifier inventory-db`

## Security Best Practices

1. **Security Groups:** Restrict SSH (port 22) to your IP only. Restrict Database (port 5432) to the EC2 Security Group only.
2. **IAM Roles:** Never store AWS credentials on the EC2 instance. Use IAM Roles.
3. **Secrets:** Pass sensitive data (DB passwords) as environment variables, do not hardcode them.
4. **HTTPS:** For production, set up a Load Balancer (ALB) with ACM Certificate or use Let's Encrypt on the EC2 instance (using Nginx reverse proxy) to enable HTTPS.

## Troubleshooting

### Cannot connect via SSH
- Check Security Group allows port 22 from your IP.
- Ensure you are using the correct `.pem` key file with correct permissions (`chmod 400 key.pem`).

### Container fails to start
- Check logs: `docker logs inventory-api`
- Verify environment variables (Connection Strings).

### Frontend cannot talk to Backend
- Check Browser Console (F12) for CORS errors.
- Ensure EC2 Security Group allows port 80 (HTTP) from the internet.
- Verify `VITE_API_URL` points to the correct EC2 Public IP/DNS.
