# Unity WebGL EC2 Runbook

This runbook publishes the Unity client as a WebGL site behind the existing
EC2 nginx reverse proxy so the game can be played from a browser link.

## 1. Install Unity WebGL Build Support

On the machine that will create the Unity build, install the WebGL module for
the exact editor version used by the project.

Current editor version:

- `6000.3.10f1`

You can verify the module is installed when this directory exists:

```text
<UnityEditor>/Editor/Data/PlaybackEngines/WebGLSupport
```

## 2. Build the Unity WebGL player

From the repository root:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\m" `
  -executeMethod MonET.Editor.WebGlBuild.BuildRelease
```

Optional custom output path:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "C:\m" `
  -executeMethod MonET.Editor.WebGlBuild.BuildRelease `
  -webglBuildPath "Builds\WebGL"
```

The build output is expected at:

```text
Builds/WebGL
```

## 3. Build and push the frontend image

Build the nginx image that serves the WebGL files:

```bash
docker build -f infra/webgl-frontend/Dockerfile -t <your-registry>/amagetdon-webgl:<tag> .
docker push <your-registry>/amagetdon-webgl:<tag>
```

The Dockerfile expects a completed WebGL build in `Builds/WebGL`.

## 4. Deploy to EC2 with the existing full-stack compose file

If the backend image is already deployed, reuse that same tag:

```bash
cd /home/ubuntu/S14P21A507
./infra/scripts/deploy.sh <backend-image> <your-registry>/amagetdon-webgl:<tag>
```

This keeps:

- `nginx` as the public reverse proxy
- `backend` on `/api/*`
- `frontend` as the Unity WebGL static site on `/`

## 5. Verify

From your local machine:

```bash
curl http://j14a507.p.ssafy.io/health
curl http://j14a507.p.ssafy.io/
```

Then open:

- `http://j14a507.p.ssafy.io/`

## 6. Notes

- The Unity client is configured to use the browser page origin for WebGL when
  it is hosted on the public domain, so `/api/*` requests stay same-origin.
- The WebGL build script disables Unity compression by default to keep nginx
  serving simple and predictable on the first deployment.
- HTTPS is not active on `j14a507.p.ssafy.io` yet in the current environment.
