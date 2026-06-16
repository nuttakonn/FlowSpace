# Continuous Integration and Deployment (CI/CD)

FlowSpace uses GitHub Actions to automate testing, building, and deployment processes. This ensures code quality on every pull request and enables seamless, zero-touch deployments to production.

## 1. CI Pipeline (`ci.yml`)

The Continuous Integration pipeline runs automatically on every **Pull Request** targeting the `main` branch. 

### Jobs:
1. **Build & Test API (.NET)**:
   - Sets up the .NET 10 SDK.
   - Restores NuGet packages.
   - Builds the backend solution (`FlowSpace.slnx`) in Release mode.
   - Executes all xUnit test suites (`FlowSpace.UnitTests`, `FlowSpace.IntegrationTests`) to prevent regressions.

2. **Build Web (Next.js)**:
   - Sets up Node.js 22.
   - Installs frontend dependencies using `npm ci`.
   - Runs a production build of the Next.js application to catch TypeScript or Linting errors.

*A Pull Request cannot be merged unless both CI jobs pass successfully.*

---

## 2. CD Pipeline (`cd.yml`)

The Continuous Deployment pipeline runs automatically whenever code is pushed (or a PR is merged) to the `main` branch.

### Strategy:
FlowSpace utilizes a remote SSH deployment strategy. Instead of building massive Docker images on GitHub Actions and pushing them to a registry (which can be slow and costly), the pipeline triggers the production server to securely pull the latest code and rebuild the containers locally using `docker-compose`.

### Workflow:
1. SSH into the production server using a secure, private key.
2. Navigate to the application deployment directory.
3. Perform a `git reset --hard` to synchronize with the `main` branch.
4. Execute `docker-compose up --build -d` to rebuild the API and Web containers with the new code and seamlessly restart the stack.
5. Automatically prune old Docker images to maintain server storage health.

---

## 3. GitHub Secrets Configuration

To enable the CD pipeline, you must configure the following **Repository Secrets** in your GitHub repository (`Settings` -> `Secrets and variables` -> `Actions`):

| Secret Name | Description | Example |
| :--- | :--- | :--- |
| `DEPLOY_HOST` | The IP address or domain name of your production server. | `192.168.1.100` or `api.flowspace.app` |
| `DEPLOY_USER` | The SSH username used to access the server. | `ubuntu` or `root` |
| `DEPLOY_KEY` | The private SSH key (PEM format) corresponding to the public key on the server. | `-----BEGIN RSA PRIVATE KEY-----...` |
| `DEPLOY_PATH` | The absolute path on the server where the repository was initially cloned. | `/var/www/flowspace` |

---

## 4. Production Environment Management

The CI/CD pipeline deliberately **does not** handle the injection of the `.env` file containing application secrets (like `POSTGRES_PASSWORD` or `JWT_SECRET`). 

**Security Best Practice:** 
You must manually SSH into the production server and create/update the `.env` file in the `DEPLOY_PATH` directory. The `docker-compose.production.yml` file will automatically read this file when the CD pipeline triggers a rebuild.
