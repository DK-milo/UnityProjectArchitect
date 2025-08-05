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

**DLL not found errors:**
- Verify DLLs are in `Runtime/Plugins/` folder
- Check assembly definition references

**Type not found errors:**
- Ensure assembly definitions include DLL references