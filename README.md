# Inventory Management System

A comprehensive inventory tracking web application built with React frontend and .NET 10 API backend, designed for AWS deployment.

## Features

- **Item Management**: Create, edit, and delete items with properties like name, price, SKU, and description
- **Location Management**: Organize items across multiple locations with hierarchical structure (parent-child relationships)
- **Inventory Tracking**: Track item quantities at different locations
- **Dynamic Properties**: Add custom properties to both items and locations
- **Flexible Design**: Supports both factory and home inventory management

## Technology Stack

### Frontend
- React 18 with TypeScript
- Vite for build tooling
- React Router for navigation
- Axios for API communication
- Responsive design optimized for desktop

### Backend
- .NET 8.0 (compatible with .NET 10 patterns)
- ASP.NET Core Web API
- Entity Framework Core for data persistence
- SQL Server / PostgreSQL / In-Memory database support
- RESTful API architecture

### Testing
- xUnit for backend unit tests
- Vitest for frontend testing
- In-memory database for isolated testing

### AWS Deployment
- **Elastic Beanstalk**: Application hosting and CI/CD
- **EC2**: Application instances
- **S3**: Static file storage
- **RDS**: Database hosting (PostgreSQL/SQL Server)

## Project Structure

```
cloud-computing-project/
├── backend/
│   ├── Controllers/          # API controllers
│   ├── Models/              # Data models
│   ├── Data/                # DbContext
│   ├── Tests/               # Unit tests
│   ├── .ebextensions/       # Beanstalk configuration
│   ├── Dockerfile           # Container configuration
│   └── Program.cs           # Application entry point
├── frontend/
│   ├── src/
│   │   ├── components/      # React components
│   │   ├── App.tsx          # Main application
│   │   ├── api.ts           # API client
│   │   └── types.ts         # TypeScript definitions
│   ├── Dockerfile           # Container configuration
│   └── package.json         # Dependencies
└── docker-compose.yml       # Local development setup
```

## Database Schema

### Items
- Id, Name, Price, Description, SKU
- Timestamps: CreatedAt, UpdatedAt

### Locations
- Id, Name, Description, Address, LocationType
- ParentLocationId (for hierarchical structure)
- Timestamps: CreatedAt, UpdatedAt

### ItemLocations (Junction Table)
- Id, ItemId, LocationId, Quantity
- Timestamps: CreatedAt, UpdatedAt

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Node.js 18+ and npm
- Docker (optional, for containerized development)

### Backend Setup

1. Navigate to the backend directory:
```powershell
cd backend
```

2. Restore dependencies:
```powershell
dotnet restore
```

3. Update connection string in `appsettings.json` (or leave empty for in-memory database):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  }
}
```

4. Run the application:
```powershell
dotnet run
```

The API will be available at `http://localhost:5000`

5. Run tests:
```powershell
cd Tests
dotnet test
```

### Frontend Setup

1. Navigate to the frontend directory:
```powershell
cd frontend
```

2. Install dependencies:
```powershell
npm install
```

3. Create a `.env` file (optional):
```
VITE_API_URL=http://localhost:5000/api
```

4. Run the development server:
```powershell
npm run dev
```

The application will be available at `http://localhost:3000`

5. Run tests:
```powershell
npm test
```

### Docker Development

Run the entire stack with Docker Compose:

```powershell
docker-compose up --build
```

- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- API Documentation: http://localhost:5000/swagger

## API Endpoints

### Items
- `GET /api/items` - Get all items
- `GET /api/items/{id}` - Get item by ID
- `POST /api/items` - Create new item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item

### Locations
- `GET /api/locations` - Get all locations
- `GET /api/locations/{id}` - Get location by ID
- `POST /api/locations` - Create new location
- `PUT /api/locations/{id}` - Update location
- `DELETE /api/locations/{id}` - Delete location

### Item Locations
- `GET /api/itemlocations` - Get all item-location mappings
- `GET /api/itemlocations/{id}` - Get by ID
- `GET /api/itemlocations/bylocation/{locationId}` - Get items at location
- `GET /api/itemlocations/byitem/{itemId}` - Get locations for item
- `POST /api/itemlocations` - Add item to location
- `PUT /api/itemlocations/{id}` - Update quantity
- `DELETE /api/itemlocations/{id}` - Remove item from location

## AWS Deployment

### Prerequisites
- AWS CLI configured with appropriate credentials
- Elastic Beanstalk CLI (eb cli)
- AWS account with necessary permissions

### Deploying to AWS Elastic Beanstalk

1. **Initialize Elastic Beanstalk application:**

```powershell
cd backend
eb init -p "64bit Amazon Linux 2 v2.6.1 running .NET Core" inventory-api --region us-east-1
```

2. **Create environment:**

```powershell
eb create inventory-api-prod --instance-type t3.micro --database
```

3. **Deploy backend:**

```powershell
dotnet publish -c Release -o publish
Compress-Archive -Path publish\* -DestinationPath publish.zip
eb deploy
```

4. **Frontend - Build and upload to S3:**

```powershell
cd ..\frontend
npm run build

# Upload to S3 bucket (create bucket first)
aws s3 mb s3://inventory-frontend-bucket
aws s3 sync dist/ s3://inventory-frontend-bucket --acl public-read

# Configure S3 for static website hosting
aws s3 website s3://inventory-frontend-bucket --index-document index.html --error-document index.html
```

5. **Configure CloudFront (optional) for CDN:**

```powershell
# Create CloudFront distribution pointing to S3 bucket
aws cloudfront create-distribution --origin-domain-name inventory-frontend-bucket.s3.amazonaws.com
```

### Environment Variables

Set these in Elastic Beanstalk environment configuration:

- `ASPNETCORE_ENVIRONMENT`: Production
- `ConnectionStrings__DefaultConnection`: Your RDS connection string
- CORS origins should include your CloudFront or S3 website URL

## CI/CD with AWS

Elastic Beanstalk provides automatic deployment when you push code:

```powershell
# Backend deployment
cd backend
dotnet publish -c Release -o publish
Compress-Archive -Path publish\* -DestinationPath publish.zip -Force
eb deploy

# Frontend deployment
cd frontend
npm run build
aws s3 sync dist/ s3://inventory-frontend-bucket --delete
aws cloudfront create-invalidation --distribution-id YOUR_DIST_ID --paths "/*"
```

## Testing

### Backend Tests
```powershell
cd backend/Tests
dotnet test
```

### Frontend Tests
```powershell
cd frontend
npm test
```

## Architecture Decisions

- **Entity Framework Core**: Provides database abstraction supporting multiple databases
- **In-Memory Database**: Available for development without database setup
- **RESTful API**: Standard HTTP methods for CRUD operations
- **React SPA**: Single-page application for smooth user experience
- **TypeScript**: Type safety in frontend development
- **Docker**: Containerization for consistent environments

## Security Considerations

- CORS configured for specific origins
- HTTPS enforced in production
- SQL injection prevented through Entity Framework parameterization
- Input validation on both client and server
- Environment-specific configuration files

## Performance Optimizations

- Database indexing on foreign keys
- Eager loading of related entities to reduce queries
- React component memoization
- Static asset caching with nginx
- Gzip compression enabled

## Future Enhancements

- User authentication and authorization
- Role-based access control
- Advanced reporting and analytics
- Barcode/QR code scanning
- Mobile responsive design
- Real-time inventory updates with SignalR
- Export to CSV/Excel functionality
- Image upload for items
- Audit logging

## License

MIT License

## Contributors

Created for CSCI 4650 - Cloud Computing project
