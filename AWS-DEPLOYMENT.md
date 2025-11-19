# AWS Deployment Guide

## Prerequisites

1. AWS Account
2. AWS CLI installed and configured
3. EB CLI installed: `pip install awsebcli`
4. .NET 9.0 SDK
5. Node.js 18+

## Backend Deployment to Elastic Beanstalk

### Step 1: Initialize EB Application

```powershell
cd backend
eb init
```

Select:
- Region: Your preferred region (e.g., us-east-1)
- Application name: inventory-api
- Platform: .NET 9 running on 64bit Amazon Linux 2023
- CodeCommit: No

### Step 2: Create Environment

```powershell
# Create a single-instance environment to stay within Free Tier (avoids Load Balancer costs)
eb create inventory-api-prod `
  --instance-type t3.micro `
  --single `
  --database `
  --database.engine postgres `
  --database.size 5 `
  --database.instance db.t3.micro `
  --database.username admin
```

You'll be prompted for a database password. Save this securely.

### Step 3: Configure Environment Variables

```powershell
eb setenv `
  ASPNETCORE_ENVIRONMENT=Production `
  ConnectionStrings__DefaultConnection="Host=YOUR_RDS_ENDPOINT;Database=inventory;Username=admin;Password=YOUR_PASSWORD"
```

### Step 4: Deploy Application

```powershell
# Build and publish
dotnet publish -c Release -o publish

# Create deployment package
cd publish
Compress-Archive -Path * -DestinationPath ..\deploy.zip -Force
cd ..

# Deploy to EB
eb deploy
```

### Step 5: Configure CORS

After deployment, update the CORS policy in your code to include the EB URL:

```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "https://your-cloudfront-domain.cloudfront.net",
    "https://inventory-api-prod.us-east-1.elasticbeanstalk.com"
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
"VITE_API_URL=https://inventory-api-prod.us-east-1.elasticbeanstalk.com/api" | Out-File -FilePath .env -Encoding utf8

# Build
npm install
npm run build

# Deploy to S3
aws s3 sync dist/ s3://$bucketName --delete --acl public-read
```

### Step 4: Create CloudFront Distribution

```powershell
# Create distribution (this returns a JSON with DistributionId)
aws cloudfront create-distribution `
  --origin-domain-name $bucketName.s3.amazonaws.com `
  --default-root-object index.html

# Note the DomainName from the output
```

Create CloudFront config file `cf-config.json`:

```json
{
  "CallerReference": "inventory-frontend-2024",
  "Origins": {
    "Quantity": 1,
    "Items": [
      {
        "Id": "S3-inventory-frontend",
        "DomainName": "YOUR_BUCKET_NAME.s3.amazonaws.com",
        "S3OriginConfig": {
          "OriginAccessIdentity": ""
        }
      }
    ]
  },
  "DefaultCacheBehavior": {
    "TargetOriginId": "S3-inventory-frontend",
    "ViewerProtocolPolicy": "redirect-to-https",
    "AllowedMethods": {
      "Quantity": 2,
      "Items": ["GET", "HEAD"],
      "CachedMethods": {
        "Quantity": 2,
        "Items": ["GET", "HEAD"]
      }
    },
    "ForwardedValues": {
      "QueryString": false,
      "Cookies": {
        "Forward": "none"
      }
    },
    "MinTTL": 0,
    "DefaultTTL": 86400,
    "MaxTTL": 31536000
  },
  "Enabled": true,
  "Comment": "Inventory Frontend Distribution"
}
```

### Step 5: Update CORS

Update backend CORS to include CloudFront domain:

```powershell
cd backend
# Edit Program.cs to add CloudFront domain
# Redeploy
eb deploy
```

## RDS Database Setup

### Create PostgreSQL RDS Instance

If not created with EB:

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
  --db-subnet-group-name default `
  --backup-retention-period 7 `
  --port 5432
```

### Run Migrations

```powershell
cd backend

# Update connection string in appsettings.json
# Run migrations
dotnet ef database update
```

## Continuous Deployment

### GitHub Actions (Optional)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to AWS

on:
  push:
    branches: [ main ]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 9.0.x
      
      - name: Publish
        run: |
          cd backend
          dotnet publish -c Release -o publish
      
      - name: Deploy to EB
        uses: einaregilsson/beanstalk-deploy@v20
        with:
          aws_access_key: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws_secret_key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          application_name: inventory-api
          environment_name: inventory-api-prod
          version_label: ${{ github.sha }}
          region: us-east-1
          deployment_package: backend/publish.zip

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

```powershell
# View EB logs
eb logs

# Stream logs
eb logs --stream
```

### Set up CloudWatch Alarms

```powershell
# High CPU alarm
aws cloudwatch put-metric-alarm `
  --alarm-name inventory-api-high-cpu `
  --alarm-description "Alert when CPU exceeds 80%" `
  --metric-name CPUUtilization `
  --namespace AWS/EC2 `
  --statistic Average `
  --period 300 `
  --threshold 80 `
  --comparison-operator GreaterThanThreshold
```

## Cost Optimization

- **Use Single Instance Type:** The `--single` flag in `eb create` avoids creating an Application Load Balancer (ALB), which can be costly if the free tier limit (750 hours/month) is exceeded.
- **Instance Types:** Use `t3.micro` for EC2 and `db.t3.micro` for RDS (Free Tier eligible).
- **Database:** Ensure `Multi-AZ` is disabled for development databases to avoid doubling costs.
- **Storage:** Clean up old EB application versions and unused S3 buckets.
- **Scheduling:** Schedule non-production environments to stop during off-hours using AWS Instance Scheduler or simple Lambda scripts.

## Security Best Practices

1. Enable HTTPS/SSL certificates
2. Use VPC for RDS isolation
3. Enable AWS WAF on CloudFront
4. Rotate credentials regularly
5. Enable CloudTrail for auditing
6. Use IAM roles instead of access keys where possible
7. Enable S3 bucket encryption

## Troubleshooting

### Backend won't start
```powershell
eb logs
# Check for connection string errors
# Verify security groups allow database connections
```

### CORS errors
- Ensure backend CORS includes frontend domain
- Check CloudFront is forwarding headers correctly

### Database connection errors
- Verify security group rules
- Check connection string format
- Ensure RDS is in same VPC as EB environment
