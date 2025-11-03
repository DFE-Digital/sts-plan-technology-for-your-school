# Site Offline Microsite

A lightweight maintenance page microsite for Plan Technology for Your School.

## Purpose

This microsite displays a "Service Unavailable" page to users during maintenance windows. It's deployed to Azure Container Apps before database upgrades and other deployment steps, ensuring users see a proper maintenance message instead of errors.

## Features

- GOV.UK Design System styling
- DfE branding
- Minimal dependencies for fast startup
- Health check endpoint at `/health`
- Lightweight Docker image

## Local Development

### Run locally
```bash
cd src/Dfe.PlanTech.Web.SiteOfflineMicrosite
dotnet run
```

The site will be available at `http://localhost:5000`

### Build Docker image
```bash
docker build -f src/Dfe.PlanTech.Web.SiteOfflineMicrosite/Dockerfile -t site-offline .
docker run -p 8080:8080 site-offline
```

## Deployment Flow

1. **Build Phase** (`matrix-deploy.yml`):
   - Builds the site offline microsite image
   - Publishes to GitHub Container Registry (GCR)

2. **Deploy Phase** (`deploy-image.yml`):
   - **Step 1**: Deploy maintenance site to Azure Container Apps
   - **Step 2**: Run database upgrader (with environment protection requiring approval)
   - **Step 3**: Deploy actual application (overwrites maintenance site)

## CI/CD Integration

The microsite is automatically built and deployed as part of the standard deployment pipeline:

- Built in parallel with the main application image
- Deployed first to show maintenance message
- Overwritten by the actual application once deployment completes

## Customization

To customize the maintenance message, edit the HTML in `Program.cs` within the `GetMaintenanceHtml()` method.

## Technical Details

- **Framework**: ASP.NET Core 9.0 (minimal API)
- **Port**: 8080
- **Health Check**: `/health` endpoint for container health monitoring
- **User**: Runs as non-root user `dotnet` in container

