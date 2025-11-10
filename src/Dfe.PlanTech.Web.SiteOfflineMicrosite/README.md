**# Site Offline Microsite

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

## Customisation

The maintenance message can be customised via environment variables without requiring a full redeployment. This is particularly useful during deployments when you need to update timing information or provide specific details about the maintenance window.

### Configuration

Set the `Maintenance__MessageParagraphs__*` environment variables with an array of strings. Each string will be displayed as a separate paragraph on the maintenance page.

**Example configuration:**

```bash
Maintenance__MessageParagraphs__0="The service will be unavailable from 5pm on Monday 4th November."
Maintenance__MessageParagraphs__1="You will be able to use the service from 9am on Tuesday 5th November."
```

**Default behaviour:**

If no custom messages are configured, a generic maintenance message is displayed:

> You'll be able to use the service later.
>
> The service is temporarily unavailable for scheduled maintenance. This normally lasts only a few minutes. Please check back later.

