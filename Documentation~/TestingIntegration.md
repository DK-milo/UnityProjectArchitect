# Testing Unity Project Architect Integration

## Setup for Testing

**Recommended Approach:**
1. Create a new Unity project (Unity 6000.0.53 or later)
2. Import the package: `Window > Package Manager > + > Add package from disk`
3. Navigate to: `UnityProjectArchitect/Packages/com.unitprojectarchitect.core`
4. Select `package.json`

## Manual Testing in Unity Editor

1. Go to `Window > Unity Project Architect`
2. Create a new Project Data Asset
3. Test these features:
   - **Analyze Project** - Tests ProjectAnalyzer service
   - **Export Documentation** - Tests ExportService 
   - **Generate Documentation** - Tests content generation

## Automated Tests

1. Open `Window > General > Test Runner`
2. Switch to `PlayMode` tab
3. Look for `UnityProjectArchitect.Unity.Tests`
4. Run `DLLIntegrationTest` suite

## Expected Console Messages

When testing successfully, you should see:
- `Unity Project Architect Service Bridge initialized with DLL services`
- `✅ Project validation completed`
- `✅ Export service supports X formats`
- `✅ All DLL types are accessible from Unity`

## Troubleshooting

**"Service not initialized" errors:**
- Check console for initialization error messages
- Ensure all three DLLs are in `Runtime/Plugins/` folder

**Interface implementation errors:**
- Re-import the package if you see interface mismatch errors
- Check that all DLL files are the latest version

**DLL not found errors:**
- Verify DLLs are in `Runtime/Plugins/` folder: Core.dll, AI.dll, Services.dll
- Check assembly definition references include all three DLLs

**Type not found errors:**
- Ensure assembly definitions include DLL references
- Unity 6000.0.53+ required for .NET Standard 2.1 compatibility