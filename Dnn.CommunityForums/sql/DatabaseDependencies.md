# DNN Community Forums database dependency graph

This document summarizes the forums schema dependency state after applying all versioned `*.SqlDataProvider` scripts in order (excluding `Uninstall.SqlDataProvider`).

## DNN platform tables referenced

- `{objectQualifier}Modules`
- `{objectQualifier}Users`
- `{objectQualifier}UserPortals`

## Core forums dependency graph (import/export relevant)

```text
Modules
  ├─ activeforums_Groups (ModuleId)
  ├─ activeforums_Forums (ModuleId)
  ├─ activeforums_Content (ModuleId)
  ├─ activeforums_Tags (ModuleId)
  └─ (additional module-scoped tables)

activeforums_Groups
  └─ activeforums_Forums (ForumGroupId logical dependency)

activeforums_Content
  ├─ activeforums_Topics (ContentId, ON DELETE CASCADE)
  └─ activeforums_Replies (ContentId, ON DELETE CASCADE)

activeforums_Topics
  ├─ activeforums_Replies (TopicId)
  ├─ activeforums_ForumTopics (TopicId)
  ├─ activeforums_Topics_Tags (TopicId)
  ├─ activeforums_Topics_Categories (TopicId)
  └─ activeforums_Topics_Ratings / activeforums_Topics_Tracking / poll/url tables

activeforums_Forums
  └─ activeforums_ForumTopics (ForumId)

activeforums_Replies
  └─ activeforums_ForumTopics (LastReplyId nullable)

activeforums_Tags
  └─ activeforums_Topics_Tags (TagId)
```

## Export/import order used for referential integrity

- Export/import in increasing dependency order:
  1. `activeforums_Groups`, `activeforums_Tags`
  2. `activeforums_Forums`
  3. `activeforums_Content`
  4. `activeforums_Topics`
  5. `activeforums_Replies`
  6. `activeforums_ForumTopics`
  7. `activeforums_Topics_Tags`

## Additional dependency notes

- User-profile scoped tables depend on DNN users/user-portals:
  - `activeforums_UserProfiles` → `UserPortals`
  - `activeforums_UserBadges` / `activeforums_UserMentions` → `activeforums_UserProfiles`
- Many module-scoped support tables depend on `Modules` (settings, subscriptions, filters, queue, badges, categories, etc.).
- FK updates in later versions replace legacy FK names with the current names (for example in `07.00.07`, `08.02.00`, `09.04.00`).

## Key script references

- Initial core FK creation: `04.00.00.SqlDataProvider`
- Content/topic/reply/tag/module cascade FK normalization: `07.00.07.SqlDataProvider`
- Reply/forum-topic FK corrections: `08.02.00.SqlDataProvider`
- User profile FK to `UserPortals`: `08.02.00.SqlDataProvider` / `08.02.04.SqlDataProvider`
- Topics-tags FK refresh: `09.04.00.SqlDataProvider`
- Badge/user-badge/user-mention FK additions: `09.02.00.SqlDataProvider`, `09.02.03.SqlDataProvider`, `09.03.00.SqlDataProvider`
