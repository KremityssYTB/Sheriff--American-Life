# Cloud build notes

## Recommended cloud build path: Unity Build Automation

Use Unity's hosted Build Automation service from the Unity Cloud Dashboard. This is the preferred $0 route for the phone-only workflow; it avoids renting a remote Windows PC and does not require placing a Unity Personal `.ulf` in GitHub Actions secrets for the Unity-managed build path.

- Unity Editor: 6000.3.25f1 (Unity 6.3 LTS)
- URP: 17.3.0
- Source: GitHub `KremityssYTB/Sheriff--American-Life`, branch `main`
- Project root: repository root (leave project subfolder blank)
- Build target: iOS
- Builder: macOS
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt`
- Free allowance: Unity's 2026 pricing page lists 100 Mac build minutes per month for iOS/macOS builds. Free-tier access is blocked when limits are exhausted if no payment method is on file. Do not add billing details or enable pay-as-you-go.

### One-time owner setup in Unity Cloud

1. Sign in at https://cloud.unity.com/ using the Unity ID for the project.
2. Open DevOps / Build Automation and create or launch the free Unity Cloud Personal project.
3. Connect GitHub and select `KremityssYTB/Sheriff--American-Life`.
4. Configure the iOS target as above. Start with manual builds.
5. Run the first build and inspect its log before enabling automatic builds.

Unity's GitHub connection requires a token. For manual builds of this public repo, use the minimum public-repository read permission. If a fine-grained token is used, grant `Contents: Read-only` only for this repo. Automatic builds also require webhook write permission, so do not enable them until the manual build is successful. Create and enter the token directly in Unity's dashboard; never share it in chat.

References:
- Free allowance: https://unity.com/products/pricing-updates
- GitHub connection: https://docs.unity.com/en-us/build-automation/get-started-with-build-automation/connect-your-version-control-system
- Build configuration: https://docs.unity.com/en-us/build-automation/basic-build-configuration/overview
- Supported Unity versions: https://docs.unity.com/en-us/build-automation/reference/supported-unity-versions

### iOS output limitation

Unity's iOS build step generates an Xcode project. Xcode then builds that project into an app. A distributable/installable IPA requires a separate Xcode packaging and Apple signing step. Do not call the Xcode project an IPA, and do not claim a signed or installable build until verified.

## Existing GitHub Actions / GameCI workflow

The existing manual workflow at `.github/workflows/build-ios-export.yml` is an alternative path. It requires the GitHub Actions secret `UNITY_LICENSE` containing a valid Unity Personal license. That secret is not configured yet. Unity Personal activation must be performed through Unity Hub; the old manual `.alf` route is not supported for Personal.

## Project readiness and validation

- The project targets Unity 6.3 (6000.3.25f1) and URP 17.3.0.
- `Assets/Editor/ProjectSetup.cs` generates the URP assets and `Assets/Scenes/Main.unity` when invoked.
- An iOS pre-build hook now creates the generated scene when missing, verifies the generated mobile URP asset, assigns it to Graphics/Quality settings, and enforces the iPhone+iPad, IL2CPP, Metal, bundle ID, and auto-rotation configuration. This hook is committed source and still requires a real Unity cloud build to validate.
- No Unity compile, UBA build, Xcode build, device install, or runtime test is claimed until that exact step has completed successfully.
