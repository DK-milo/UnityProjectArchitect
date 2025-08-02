# Unity Project Architect - Development Rules & Actions

**Version:** 1.1  
**Last Updated:** August 2, 2025  
**Purpose:** Standardized development workflow and custom actions for Unity Project Architect package

---

## Overview

This document defines the complete development pipeline for implementing features and stages in the Unity Project Architect project, plus custom actions that can be triggered instantly. This process ensures consistent quality, documentation, and progress tracking across all development phases.

---

## 🚀 **Custom Actions System**

**Usage Format:** `Rules.md.[ActionName]`

### Available Actions

#### **Rules.md.CommitMessage**
**Purpose:** Generate a comprehensive commit message following established format without executing any git commands  
**Usage:** Simply type `Rules.md.CommitMessage`  
**Output:** Ready-to-use commit message text for manual git commit

**Format Template:**
```
Implement Step X: [Feature Name] - [Brief Description]

- Add [File1.cs] [description of what it does]
- Add [File2.cs] [description of what it does]  
- Add [File3.cs] [description of what it does]
[... continue for all significant files/changes]

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

#### **Rules.md.UpdateProgress**
**Purpose:** Update DevelopmentRoadmapProgress.md with current completion status  
**Usage:** Simply type `Rules.md.UpdateProgress`  
**Output:** Updates roadmap document with latest progress

#### **Rules.md.NextStep**
**Purpose:** Identify and display the next step to implement from the roadmap with implementation preview  
**Usage:** Simply type `Rules.md.NextStep`  
**Output:** 
- Clear indication of next step with format: `"Help me implement Step X: [Feature Name]"`
- **Implementation Summary:** Overview of what will be implemented
- **Process Preview:** Summary of pipeline steps that will be executed
- **Prerequisites:** What needs to be reviewed before starting (coding conventions, existing patterns, etc.)
- **Expected Deliverables:** List of files/components that will be created
- **Estimated Time:** Time estimate for the step

#### **Rules.md.DocumentUpdate**
**Purpose:** Update document.md with latest commit entry  
**Usage:** Simply type `Rules.md.DocumentUpdate`  
**Output:** Updates commit history following established patterns

### Adding New Actions
To add new custom actions, follow this pattern:
1. Add action name and description to this list
2. Define clear input/output behavior
3. Maintain consistent `Rules.md.[ActionName]` format

---

## Core Development Pipeline

*The pipeline includes interactive checkpoints for user control and review*

### 🔄 **Interactive Workflow Summary**
1. **User Request:** `"Help me implement Step X: [Feature Name]"`
2. **Claude Executes:** Steps 1-4 (Request → Implementation → Testing → Quality Check)
3. **🛑 CHECKPOINT 1:** Claude asks: *"Want to continue?"*
   - **"Yes"** → Continue to Steps 5-7
   - **"No"** → Stop for review
4. **Claude Executes:** Steps 5-7 (Documentation → Dependencies → Performance)
5. **🛑 CHECKPOINT 2:** Claude asks: *"Ready to finalize?"*
   - **"Yes"** → Continue to Steps 8-9
   - **"No"** → Stop for final review
6. **Claude Executes:** Steps 8-9 (Commit Message + Documentation Updates)
7. **Complete:** Ready for manual git push

### 1. 🎯 **Request Next Implementation Step**

**Process:**
- Ask for the next step using the established format: 
  - `"Help me implement Step X: [Feature Name]"`
  - Example: `"Help me implement Step 2B: Documentation Section Generators"`

**Requirements:**
- Reference the specific step from DevelopmentRoadmapProgress.md
- Provide context from previous completed steps if needed
- Specify any particular requirements or constraints

**Success Criteria:**
- Clear step identification with proper naming convention
- Alignment with overall project roadmap
- Prerequisites are met from previous steps

### 1.5. 📋 **Review Project Context & Coding Conventions** *(NEW)*

**Process:**
- **MANDATORY:** Before any code implementation, review all relevant documentation:
  - Read `CodingConventions.md` for coding standards and variable declaration rules
  - Review `ProductRequirementsDocument.md` for business requirements
  - Check existing codebase patterns in similar files
  - Understand established naming conventions and architectural patterns

**Critical Requirements:**
- **ALWAYS use explicit types instead of `var` keyword** (per CodingConventions.md Section 2.2)
- Follow established naming conventions (PascalCase, camelCase, underscore prefixes)
- Use proper namespace organization and file headers
- Implement comprehensive error handling and logging patterns
- Follow Unity-specific ScriptableObject and Editor patterns

**Success Criteria:**
- All new code follows established coding conventions
- No `var` keyword usage (use explicit types like `string`, `List<T>`, etc.)
- Consistent with existing codebase architecture
- Proper error handling and validation patterns implemented

### 2. 🔧 **Implementation Phase**

**Process:**
- Follow the detailed technical prompts from Prompts.md "Development Props" section
- Implement code following established patterns and conventions
- Maintain consistent architecture and coding standards

**Requirements:**
- Use established interfaces and data models
- Follow SOLID principles and Unity best practices
- Include comprehensive error handling and logging
- Maintain async/await patterns where appropriate

**Success Criteria:**
- Code compiles without errors or warnings
- Implementation matches interface contracts
- Follows established coding conventions and patterns

### 3. 🧪 **Comprehensive Testing**

*Testing approach adapts based on current development phase and implementation context*

#### 3.1 Compilation Testing
**Current Phase (Pure C# Implementation):**
- Verify all new code compiles successfully in IDE (Rider/Visual Studio)
- Check for compiler errors or warnings
- Validate namespace and assembly references

**Unity Integration Phase (When Applicable):**
- Unity Editor compilation verification
- Unity console error/warning checks
- Assembly definition validation

**Tools & Approaches:**
- **Current:** IDE compilation, .NET framework validation
- **Later:** Unity Editor compilation, Assembly definition validation

**Success Criteria:**
- Zero compilation errors in current environment
- Zero warnings (or documented acceptable warnings)
- All references resolve correctly

#### 3.2 Functionality Testing
**Current Phase (Service Logic):**
- Test core service functionality through direct instantiation
- Validate interface implementations work as expected
- Test data model serialization/deserialization
- Verify async operations complete successfully

**Unity Integration Phase (When Applicable):**
- ScriptableObject creation and serialization in Unity
- Editor window functionality testing
- Asset database integration testing

**Tools & Approaches:**
- **Current:** Direct C# testing, mock data validation, async operation testing
- **Later:** Unity Editor testing, ScriptableObject validation, UI functionality

**Success Criteria:**
- All public methods execute without exceptions
- Expected outputs are generated correctly
- Data models serialize/deserialize properly
- Async operations complete without hanging

#### 3.3 Integration Testing
**Current Phase (Service Integration):**
- Test interaction between service interfaces
- Validate data flow through analysis pipeline
- Ensure service dependencies resolve correctly
- Test error handling and graceful degradation

**Unity Integration Phase (When Applicable):**
- End-to-end workflow testing in Unity Editor
- Cross-component interaction in Unity environment
- Regression testing of Unity-specific features

**Tools & Approaches:**
- **Current:** Service integration testing, dependency injection validation
- **Later:** Unity Editor workflow testing, Asset pipeline integration

**Success Criteria:**
- Services integrate correctly through interfaces
- Data flows correctly through analysis pipeline
- Error handling works across service boundaries
- No regressions in existing service functionality

---

## 🛑 **CHECKPOINT 1: User Confirmation**
**After completing Steps 1-4 (Request → Implementation → Testing → Quality Check)**

**Claude will ask:** *"Tasks 1-4 completed successfully. Implementation is done and tested. **Want to continue** with validation and finalization (Steps 5-7)?"*

**User Response Options:**
- **"Yes"** → Continue to Steps 5-7
- **"No"** → Stop pipeline, allow for review/modifications
- **"Review [specific aspect]"** → Detailed explanation of specific step before continuing

---

### 4. 📋 **Code Review & Quality Check**

**Process:**
- Review code for adherence to established patterns
- Validate error handling and edge cases
- Check performance implications and memory usage
- Ensure proper documentation and comments

**Requirements:**
- Follow established coding conventions
- Include appropriate error handling
- Optimize for performance where needed
- Add XML documentation for public APIs

**Success Criteria:**
- Code follows established patterns and conventions
- Comprehensive error handling implemented
- Performance impact is acceptable
- Public APIs are properly documented

### 5. 📖 **Documentation Verification**

**Process:**
- Ensure implementation matches technical specifications
- Verify that new features are properly documented
- Update any affected documentation sections

**Requirements:**
- Implementation aligns with design specifications
- New features have appropriate documentation
- Existing documentation remains accurate

**Success Criteria:**
- Implementation matches documented specifications
- All new features are documented
- No documentation inconsistencies exist

### 6. 🏗️ **Dependency Validation**

**Process:**
- Verify all dependencies are properly managed
- Check for circular dependencies
- Validate assembly definition configurations
- Ensure proper separation of concerns

**Tools & Approaches:**
- Dependency graph analysis
- Assembly definition validation
- Interface contract verification

**Success Criteria:**
- No circular dependencies introduced
- Assembly definitions are correctly configured
- Dependencies follow established architecture

### 7. ⚡ **Performance Impact Assessment**

**Process:**
- Evaluate performance impact of new implementation
- Check memory usage patterns
- Validate async operations don't block unnecessarily
- Ensure scalability for larger projects

**Considerations:**
- Memory allocation patterns
- File I/O operations efficiency
- Async operation performance
- Large project scalability

**Success Criteria:**
- No significant performance degradation
- Memory usage remains within acceptable limits
- Async operations are properly optimized
- Implementation scales appropriately

---

## 🛑 **CHECKPOINT 2: Final Confirmation**
**After completing Steps 5-7 (Documentation → Dependencies → Performance)**

**Claude will ask:** *"Validation and finalization completed successfully (Steps 5-7). Ready to **finalize with commit message and documentation updates**?"*

**User Response Options:**
- **"Yes"** → Continue to Steps 8-9 (Commit Message + Documentation Updates)
- **"No"** → Stop pipeline, allow for final review/modifications
- **"Skip documentation updates"** → Generate commit message only (Step 8)
- **"Review validation results"** → Detailed summary of validation findings

---

### 8. 💾 **Create Commit Message**

**Process:**
- Generate comprehensive commit message following established format
- Include all implemented features and changes
- Reference relevant files and components
- Follow the structure from document.md examples
- **IMPORTANT:** Only provide the commit message text, do NOT execute git commands

**Format Template:**
```
Implement Step X: [Feature Name] - [Brief Description]

- Add [File1.cs] [description of what it does]
- Add [File2.cs] [description of what it does]  
- Add [File3.cs] [description of what it does]
[... continue for all significant files/changes]

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Success Criteria:**
- Message accurately describes all changes
- Follows established commit message format
- Includes all relevant technical details
- Provided as text only for user to manually commit

### 9. 📚 **Update Project Documentation**

#### 9.1 Required Document Updates

**DevelopmentRoadmapProgress.md:**
- Mark completed step as ✅ **COMPLETED**
- Update progress percentages and time estimates
- Move next step to "in_progress" status
- Update "Next Immediate Tasks" section

**Prompts.md - Development Props Section:**
- Add new detailed prompt for the completed step
- Include technical implementation requirements
- Document step-by-step process used
- Maintain reference documentation structure

**document.md:**
- Add new commit entry with hash and description
- Update total commit count
- Maintain chronological order (newest first)

#### 9.2 Optional Document Updates (as needed)

**ProductRequirementsDocument.md:**
- Update if requirements change or are clarified
- Modify acceptance criteria if needed
- Adjust timeline estimates if necessary

**README.md:**
- Update if package capabilities change significantly
- Modify installation or usage instructions if needed

#### 9.3 Update Process
1. Review all affected documentation
2. Update progress tracking and status
3. Add reference documentation for completed work
4. Verify all links and references remain valid
5. Maintain consistent formatting and style

**Success Criteria:**
- All progress tracking is accurately updated
- Reference documentation is complete and detailed
- All document cross-references remain valid
- Documentation maintains professional quality

---

## Pipeline Validation Checklist

Before considering a step complete, verify:

- [ ] ✅ **Implementation:** Code compiles and functions correctly
- [ ] ✅ **Testing:** All testing phases pass successfully
- [ ] ✅ **Quality:** Code review and quality checks complete
- [ ] ✅ **Documentation:** Technical docs align with implementation
- [ ] ✅ **Dependencies:** No circular dependencies or conflicts
- [ ] ✅ **Performance:** No significant performance impact
- [ ] ✅ **Commit:** Message accurately describes all changes
- [ ] ✅ **Updates:** All required documentation updated

---

## Emergency Procedures

### Rollback Process
If critical issues are discovered:
1. Document the issue and root cause
2. Revert to last known good state
3. Update documentation to reflect rollback
4. Plan corrective implementation approach

### Hotfix Process
For urgent fixes outside normal pipeline:
1. Implement minimal fix required
2. Fast-track through testing phases
3. Create focused commit message
4. Update documentation with hotfix note
5. Plan proper implementation in next cycle

---

## Pipeline Metrics

Track the following for process improvement:
- **Implementation Time:** Actual vs estimated hours per step
- **Testing Coverage:** Number of issues caught in each testing phase
- **Documentation Accuracy:** Alignment between docs and implementation
- **Regression Rate:** Number of issues introduced vs resolved

---

## Next Steps Integration

This pipeline integrates with our current development approach:

**Current Status:** Stage 2 Sprint 2.2A completed
**Next Step:** Step 2B: Documentation Section Generators
**Pipeline Application:** Follow steps 1-9 for implementing remaining features

**Estimated Pipeline Time per Step:**
- Simple steps (data models): 15-30 minutes pipeline overhead
- Complex steps (services): 30-60 minutes pipeline overhead
- Integration steps (UI): 45-90 minutes pipeline overhead

---

**Document Status:** Active  
**Next Review:** After 3 pipeline iterations  
**Maintained By:** Development Team