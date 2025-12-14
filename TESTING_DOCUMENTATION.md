# Unity Game Testing Documentation

## Installation Guide

### Prerequisites
- Unity 2021.3 or later
- Unity Test Framework (comes with Unity by default)
- Visual Studio Code (optional, for terminal usage)

### Installing NSubstitute for Unity

#### Method 1: Using Unity Package Manager via Terminal/VSCode (Recommended)

**Step 1: Install OpenUPM CLI (if not already installed)**

Open VSCode terminal (View > Terminal or Ctrl+`) and run:

\`\`\`bash
# For Windows (using npm)
npm install -g openupm-cli

# For macOS/Linux
npm install -g openupm-cli
# or using Homebrew
brew install openupm/openupm/openupm-cli
\`\`\`

**Step 2: Navigate to your Unity project directory**

\`\`\`bash
cd /path/to/your/unity/project
\`\`\`

**Step 3: Add NSubstitute package**

\`\`\`bash
openupm add com.cysharp.nsubstitute
\`\`\`

**Step 4: Verify installation**

\`\`\`bash
# Check if package was added to manifest.json
cat Packages/manifest.json
\`\`\`

#### Method 2: Manual manifest.json Edit (Quick & Easy)

**Step 1: Open your project in VSCode**

\`\`\`bash
code /path/to/your/unity/project
\`\`\`

**Step 2: Edit Packages/manifest.json**

Open `Packages/manifest.json` in VSCode and add these lines:

\`\`\`json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp"
      ]
    }
  ],
  "dependencies": {
    "com.cysharp.nsubstitute": "5.1.0",
    // ... existing dependencies ...
  }
}
\`\`\`

**Step 3: Reload Unity Editor**

Unity will automatically detect the changes and install the package.

#### Method 3: Using Git URL in Unity Package Manager

**Step 1: Copy the Git URL**

\`\`\`bash
https://github.com/Cysharp/NSubstitute.git
\`\`\`

**Step 2: In Unity Editor**

1. Go to **Window > Package Manager**
2. Click **+** > **Add package from git URL**
3. Paste: `https://github.com/Cysharp/NSubstitute.git`
4. Click **Add**

#### Method 4: Manual DLL Installation

**Step 1: Download NSubstitute DLL**

\`\`\`bash
# Using curl in terminal
curl -L -o NSubstitute.zip https://github.com/nsubstitute/NSubstitute/releases/download/v5.1.0/NSubstitute.5.1.0.nupkg

# Or using wget
wget https://github.com/nsubstitute/NSubstitute/releases/download/v5.1.0/NSubstitute.5.1.0.nupkg
\`\`\`

**Step 2: Extract and copy to Unity**

\`\`\`bash
# Extract the package (it's actually a zip file)
unzip NSubstitute.5.1.0.nupkg -d NSubstitute

# Create Plugins folder if it doesn't exist
mkdir -p Assets/Plugins

# Copy DLL files
cp NSubstitute/lib/netstandard2.0/NSubstitute.dll Assets/Plugins/
cp NSubstitute/lib/netstandard2.0/Castle.Core.dll Assets/Plugins/
\`\`\`

### Installing Unity Test Framework (if not present)

**Via Terminal:**

Edit `Packages/manifest.json` and add:

\`\`\`json
{
  "dependencies": {
    "com.unity.test-framework": "1.1.33",
    // ... other dependencies ...
  }
}
\`\`\`

**Or use Unity Package Manager UI:**

1. Window > Package Manager
2. Click **+** > **Add package by name**
3. Enter: `com.unity.test-framework`
4. Click **Add**

### Verifying Installation

**Using Terminal:**

\`\`\`bash
# Check if NSubstitute is in the manifest
grep -A 2 "nsubstitute" Packages/manifest.json

# Check Unity console for errors (after opening Unity)
# Look for compilation errors in Library/Logs/Editor.log
tail -n 50 Library/Logs/Editor.log
\`\`\`

**In Unity Editor:**

1. Open **Window > General > Test Runner**
2. If NSubstitute is installed correctly, you should see no compilation errors
3. The Test Runner window will show "PlayMode" and "EditMode" tabs

## Setting Up Tests in VSCode

### Folder Structure

\`\`\`bash
YourUnityProject/
├── Assets/
│   ├── Scripts/           # Your game scripts
│   └── ...
├── Tests/
│   ├── EditMode/          # Tests that don't require Play mode
│   │   ├── PlayerDataTests.cs
│   │   ├── QuestionRandomizerTests.cs
│   │   └── AudioManagerTests.cs
│   └── PlayMode/          # Tests that require Unity runtime
│       ├── HealthScriptTests.cs
│       ├── CoinsScriptTests.cs
│       └── ...
└── Packages/
    └── manifest.json
\`\`\`

### Create Test Assembly Definitions

**For EditMode tests:**

Create `Tests/EditMode/Tests.EditMode.asmdef`:

\`\`\`json
{
  "name": "Tests.EditMode",
  "references": [
    "Assembly-CSharp"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "NSubstitute.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
\`\`\`

**For PlayMode tests:**

Create `Tests/PlayMode/Tests.PlayMode.asmdef`:

\`\`\`json
{
  "name": "Tests.PlayMode",
  "references": [
    "Assembly-CSharp",
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "NSubstitute.dll",
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
\`\`\`

## Running Tests

### Using Test Runner Window (Unity Editor)

1. Open **Window > General > Test Runner**
2. Switch between **EditMode** and **PlayMode** tabs
3. Click **Run All** to run all tests
4. Click individual test names to run specific tests
5. Right-click tests to run/debug selected tests

### Using Command Line / Terminal

**Windows:**

\`\`\`bash
# Set Unity path (adjust to your Unity installation)
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe"

# Run Edit Mode tests
%UNITY_PATH% -runTests -batchmode -projectPath "%CD%" -testPlatform EditMode -testResults "%CD%\TestResults-EditMode.xml" -logFile "%CD%\unity-test.log"

# Run Play Mode tests
%UNITY_PATH% -runTests -batchmode -projectPath "%CD%" -testPlatform PlayMode -testResults "%CD%\TestResults-PlayMode.xml" -logFile "%CD%\unity-test.log"

# Run all tests
%UNITY_PATH% -runTests -batchmode -projectPath "%CD%" -testResults "%CD%\TestResults-All.xml" -logFile "%CD%\unity-test.log"
\`\`\`

**macOS/Linux:**

\`\`\`bash
# Set Unity path (adjust to your Unity installation)
UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"

echo "Running Unity Tests..."
echo "Project: $PROJECT_PATH"

# Run tests
$UNITY_PATH \
  -runTests \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -testResults "$PROJECT_PATH/TestResults.xml" \
  -logFile "$PROJECT_PATH/unity-test.log"

echo "Tests complete! Check TestResults.xml for results"
echo "Check unity-test.log for detailed logs"
\`\`\`

**Creating a Test Runner Script (Recommended)**

Create `run-tests.sh` (macOS/Linux) or `run-tests.bat` (Windows):

\`\`\`bash
#!/bin/bash
# run-tests.sh

UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"

echo "Running Unity Tests..."
echo "Project: $PROJECT_PATH"

# Run tests
$UNITY_PATH \
  -runTests \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -testResults "$PROJECT_PATH/TestResults.xml" \
  -logFile "$PROJECT_PATH/unity-test.log"

echo "Tests complete! Check TestResults.xml for results"
echo "Check unity-test.log for detailed logs"
\`\`\`

Make it executable:

\`\`\`bash
chmod +x run-tests.sh
./run-tests.sh
\`\`\`

### Using VSCode Tasks (Recommended for VSCode Users)

Create `.vscode/tasks.json`:

\`\`\`json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Unity: Run All Tests",
      "type": "shell",
      "command": "/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity",
      "args": [
        "-runTests",
        "-batchmode",
        "-projectPath",
        "${workspaceFolder}",
        "-testResults",
        "${workspaceFolder}/TestResults.xml",
        "-logFile",
        "${workspaceFolder}/unity-test.log"
      ],
      "group": {
        "kind": "test",
        "isDefault": true
      },
      "presentation": {
        "reveal": "always",
        "panel": "new"
      }
    },
    {
      "label": "Unity: Run Edit Mode Tests",
      "type": "shell",
      "command": "/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity",
      "args": [
        "-runTests",
        "-batchmode",
        "-projectPath",
        "${workspaceFolder}",
        "-testPlatform",
        "EditMode",
        "-testResults",
        "${workspaceFolder}/TestResults-EditMode.xml",
        "-logFile",
        "${workspaceFolder}/unity-test.log"
      ],
      "group": "test"
    },
    {
      "label": "Unity: Run Play Mode Tests",
      "type": "shell",
      "command": "/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity",
      "args": [
        "-runTests",
        "-batchmode",
        "-projectPath",
        "${workspaceFolder}",
        "-testPlatform",
        "PlayMode",
        "-testResults",
        "${workspaceFolder}/TestResults-PlayMode.xml",
        "-logFile",
        "${workspaceFolder}/unity-test.log"
      ],
      "group": "test"
    }
  ]
}
\`\`\`

**Run tests in VSCode:**

1. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
2. Type "Run Task"
3. Select "Unity: Run All Tests" (or any specific test task)

Or press `Ctrl+Shift+B` to run the default test task.

### Viewing Test Results

\`\`\`bash
# View XML results
cat TestResults.xml

# View detailed logs
cat unity-test.log

# Parse XML for summary (using xmllint if available)
xmllint --format TestResults.xml

# Quick pass/fail summary
grep -E "total|passed|failed" TestResults.xml
\`\`\`

## Test Coverage

### Core Game Logic Tests

#### 1. **PlayerDataTests.cs**
Tests for player state management and persistence.

**Test Cases:**
- `SingletonInstance_ShouldBeMaintained()` - Verifies singleton pattern works correctly
- `AddScore_ShouldIncreaseScore()` - Tests score addition
- `AddCoin_ShouldIncreaseCoins()` - Tests coin collection
- `DeductCoin_ShouldDecreaseCoins()` - Tests coin spending
- `DeductCoin_ShouldNotGoBelowZero()` - Tests coin deduction boundary
- `DeductHealth_ShouldDecreaseHealth()` - Tests health damage
- `DeductHealth_CallsHealthScriptUpdate()` - Verifies UI updates on damage
- `TryPurchase_WithEnoughCoins_ShouldReturnTrue()` - Tests successful purchase
- `TryPurchase_WithoutEnoughCoins_ShouldReturnFalse()` - Tests failed purchase
- `TryPurchase_HeartType_ShouldIncreaseHealth()` - Tests heart purchase effect
- `SetCheckpoint_ShouldStorePosition()` - Tests checkpoint saving
- `GetCheckpointPosition_ShouldReturnStoredPosition()` - Tests checkpoint retrieval
- `HasCheckpoint_InitiallyFalse()` - Tests initial checkpoint state
- `ClearCheckpoint_ShouldResetFlag()` - Tests checkpoint clearing

#### 2. **QuestionRandomizerTests.cs**
Tests for math question generation.

**Test Cases:**
- `GetRandomQuestion_ShouldReturnValidQuestion()` - Tests basic question generation
- `GetRandomQuestion_AnswerShouldBeWithinBounds()` - Tests answer range validation
- `GetRandomQuestion_ShouldHaveFourOptions()` - Tests option count
- `GetRandomQuestion_ShouldHaveOneCorrectAnswer()` - Tests correct answer presence
- `GetRandomQuestion_CorrectAnswerShouldMatchExpression()` - Tests answer accuracy
- `GenerateSimple_ShouldProduceValidExpression()` - Tests simple math generation
- `GenerateAdvance_ShouldProduceTwoOperatorExpression()` - Tests complex math generation
- `GenerateOptions_ShouldContainCorrectAnswer()` - Tests option generation
- `GenerateOptions_ShouldHaveUniqueOptions()` - Tests option uniqueness

#### 3. **AudioManagerTests.cs**
Tests for audio system management.

**Test Cases:**
- `AudioManagerInstance_ShouldUseSingleton()` - Tests singleton pattern
- `PlaySfx_WithValidClip_ShouldPlay()` - Tests sound effect playback
- `PlaySfx_WithNullClip_ShouldNotCrash()` - Tests null safety
- `PlaySfx_WhenMuted_ShouldNotPlay()` - Tests mute functionality
- `SetMusicMuted_ShouldUpdateMuteState()` - Tests music mute toggle
- `SetSfxMuted_ShouldUpdateMuteState()` - Tests SFX mute toggle
- `ConvenienceMethods_ShouldCallPlaySfx()` - Tests helper methods
- `PlaySfxAtPosition_ShouldCreateTemporaryObject()` - Tests 3D audio

#### 4. **HealthScriptTests.cs**
Tests for health management and damage system.

**Test Cases:**
- `TakeDamage_ShouldDeductFromPlayerData()` - Tests damage application
- `TakeDamage_WhenInvulnerable_ShouldIgnore()` - Tests invulnerability
- `UpdateHealth_WithThreeHearts_ShowsAllHearts()` - Tests UI with full health
- `UpdateHealth_WithTwoHearts_ShowsTwoHearts()` - Tests UI with partial health
- `UpdateHealth_WithZeroHearts_ShowsGameOver()` - Tests game over state
- `ActivateShield_ShouldSetInvulnerability()` - Tests shield activation
- `UpdateScore_ShouldUpdateScoreText()` - Tests score display update
- `UpdateCoin_ShouldUpdateCoinText()` - Tests coin display update
- `PlayHurtFeedback_ShouldTriggerEffects()` - Tests damage feedback

#### 5. **CoinsScriptTests.cs**
Tests for coin collection mechanics.

**Test Cases:**
- `OnTriggerEnter_WithPlayer_ShouldAddCoin()` - Tests coin collection
- `OnTriggerEnter_WithPlayer_ShouldDestroyCoin()` - Tests coin removal
- `OnTriggerEnter_WithPlayer_ShouldPlaySound()` - Tests coin sound
- `OnTriggerEnter_WithNonPlayer_ShouldNotCollect()` - Tests collision filtering
- `OnTriggerEnter_WithNullPlayerData_ShouldNotCrash()` - Tests null safety

#### 6. **BlueEnimyTests.cs**
Tests for basic enemy AI behavior.

**Test Cases:**
- `Start_ShouldFindPlayer()` - Tests player detection setup
- `Update_WhenPlayerInRange_ShouldChase()` - Tests chase behavior
- `Update_WhenPlayerOutOfRange_ShouldStop()` - Tests idle behavior
- `OnTriggerEnter_WithStomp_ShouldDie()` - Tests stomp mechanic
- `OnTriggerEnter_WithoutStomp_ShouldDealDamage()` - Tests enemy damage
- `Die_ShouldDestroyEnemy()` - Tests enemy death
- `Die_ShouldPlayDeathEffects()` - Tests death VFX/audio
- `DealDamage_ShouldCallHealthScript()` - Tests damage dealing

#### 7. **PinkEnemyTests.cs**
Tests for charging enemy AI behavior.

**Test Cases:**
- `Start_ShouldInitializeComponents()` - Tests initialization
- `Update_WhenInRange_ShouldChasePlayer()` - Tests chase behavior
- `Update_WhenInChargeRange_ShouldInitiateCharge()` - Tests charge trigger
- `Charge_ShouldMoveTowardsPlayer()` - Tests charge movement
- `Charge_ShouldDealDamageOnContact()` - Tests charge damage
- `OnTriggerEnter_WithStomp_ShouldDie()` - Tests stomp kill
- `Die_ShouldCreateDeathVFX()` - Tests death effects

#### 8. **StompableTests.cs**
Tests for stomp mechanic on enemies.

**Test Cases:**
- `ProcessTrigger_PlayerAboveAndFalling_ShouldTriggerStomp()` - Tests valid stomp
- `ProcessTrigger_PlayerNotAbove_ShouldNotStomp()` - Tests position check
- `ProcessTrigger_PlayerNotFalling_ShouldNotStomp()` - Tests velocity check
- `HandleStomp_ShouldDestroyTarget()` - Tests enemy destruction
- `HandleStomp_ShouldPlayVFX()` - Tests visual effects
- `HandleStomp_ShouldBouncePlayer()` - Tests player bounce
- `EnsureHitboxExists_ShouldCreateHitbox()` - Tests hitbox generation

#### 9. **QuestionUIControllerTests.cs**
Tests for quiz UI management.

**Test Cases:**
- `Open_ShouldDisplayQuestion()` - Tests UI display
- `Open_ShouldSetupButtons()` - Tests button configuration
- `OnChoicePressed_ShouldSubmitAnswer()` - Tests answer submission
- `OnHintPressed_WithEnoughCoins_ShouldRemoveOptions()` - Tests hint usage
- `OnHintPressed_WithoutCoins_ShouldNotWork()` - Tests insufficient funds
- `Close_ShouldHideUI()` - Tests UI cleanup
- `HintButton_ShouldDisableAfterUse()` - Tests hint one-time use

#### 10. **FalldetectTests.cs**
Tests for fall detection and respawn system.

**Test Cases:**
- `OnTriggerEnter_WithPlayer_ShouldPauseGame()` - Tests game pause
- `OnTriggerEnter_WithPlayer_ShouldShowDeathUI()` - Tests UI display
- `RespawnPlayer_ShouldMoveToRespawnPoint()` - Tests respawn position
- `RespawnPlayer_WithCheckpoint_ShouldUseCheckpoint()` - Tests checkpoint respawn
- `RespawnPlayer_ShouldResetVelocity()` - Tests physics reset
- `RespawnPlayer_ShouldDeductHealth()` - Tests health penalty
- `RespawnPlayer_ShouldResumeGame()` - Tests game resume

#### 11. **RespawnManagerTests.cs**
Tests for respawn management system.

**Test Cases:**
- `RespawnPlayer_ShouldMovePlayerToRespawnPoint()` - Tests teleportation
- `RespawnPlayer_ShouldResetRigidbodyVelocity()` - Tests physics reset
- `RespawnPlayer_ShouldUnpauseGame()` - Tests time scale reset
- `RespawnPlayer_WithoutPlayer_ShouldReloadScene()` - Tests fallback
- `RespawnPlayer_WithoutRespawnPoint_ShouldReloadScene()` - Tests safety check

#### 12. **ResultScriptTests.cs**
Tests for level completion UI.

**Test Cases:**
- `SetupButtons_HomeButton_ShouldLoadMainMenu()` - Tests menu navigation
- `SetupButtons_RestartButton_ShouldLoadLevelSelect()` - Tests restart
- `SetupButtons_NextButton_ShouldLoadNextLevel()` - Tests progression
- `NextButton_OnLastLevel_ShouldLoadMainMenu()` - Tests end game
- `NextButton_OnLevel10_ShouldTriggerVideo()` - Tests special level

## Best Practices

### Writing New Tests

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **Use Descriptive Names**: `MethodName_Scenario_ExpectedBehavior()`
3. **Mock Unity Components**: Use NSubstitute for GameObjects, Components
4. **Test One Thing**: Each test should verify one specific behavior
5. **Clean Up**: Use `[TearDown]` to clean up test data

### Example Test Structure

\`\`\`csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and mocks
    var mockObject = Substitute.For<IComponent>();
    mockObject.Method().Returns(expectedValue);
    
    // Act - Execute the method being tested
    var result = systemUnderTest.Method();
    
    // Assert - Verify the expected outcome
    Assert.AreEqual(expectedValue, result);
}
