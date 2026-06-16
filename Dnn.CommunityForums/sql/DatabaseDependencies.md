# DNN Community Forums database dependency graph

This document summarizes the forums schema dependency state after applying all versioned `*.SqlDataProvider` scripts in order (excluding `Uninstall.SqlDataProvider`).

Enforced FK constraints are noted as **FK**. Columns that reference another table's PK without a declared constraint are noted as **logical**.

## DNN platform tables referenced

- `{objectQualifier}Modules` — referenced by most module-scoped tables (ModuleId)
- `{objectQualifier}Users` — referenced by user-scoped tables (UserId, logical)
- `{objectQualifier}UserPortals` — referenced by `activeforums_UserProfiles` (UserId + PortalId, logical)

## Complete table dependency graph

### Wave 1 — No internal forum-table dependencies (module/portal-scoped only)

| Table | Key columns | Notes |
|---|---|---|
| `activeforums_Groups` | ModuleId | Root of forum hierarchy |
| `activeforums_Tags` | ModuleId, PortalId | |
| `activeforums_Badges` | ModuleId | FK → `Modules` (cascade delete) |
| `activeforums_Content` | — | Free-standing content store; not scoped by ModuleId directly |
| `activeforums_Permissions` | PermissionsId | Standalone permission record; PermissionsId referenced by Settings/Security key chains |
| `activeforums_Settings` | ModuleId | Key/value store per module |
| `activeforums_Templates` | ModuleId | |
| `activeforums_Filters` | ModuleId, PortalId | |
| `activeforums_Ranks` | ModuleId, PortalId | |
| `activeforums_Properties` | PortalId | Module-object property definitions |
| `activeforums_Security` | ObjectId, SecurityKey | ObjectId/ObjectType are discriminated references; no enforced FK |
| `activeforums_Queue` | — | Standalone email outbox; no FK dependencies |
| `activeforums_SearchCache` | — | Transient search results; no FK dependencies |

### Wave 2 — Depends on Wave 1

| Table | Dependencies |
|---|---|
| `activeforums_Forums` | ForumGroupId **logical** → `activeforums_Groups.ForumGroupId`; ParentForumId **logical** (self-ref) → `activeforums_Forums.ForumId`; LastPostId **logical** → Content/Topic/Reply |

### Wave 3 — Depends on Wave 2

| Table | Dependencies |
|---|---|
| `activeforums_Categories` | ForumId **logical** → `activeforums_Forums.ForumId`; ForumGroupId **logical** → `activeforums_Groups.ForumGroupId` |

### Wave 4 — Depends on Content (Wave 1)

| Table | Dependencies |
|---|---|
| `activeforums_Topics` | ContentId **FK** → `activeforums_Content.ContentId` |
| `activeforums_Attachments` | ContentId **FK** → `activeforums_Content.ContentId` |
| `activeforums_Likes` | PostId **FK** → `activeforums_Content.ContentId` |
| `activeforums_UserMentions` | ContentId **FK** → `activeforums_Content.ContentId` |

### Wave 5 — Depends on Topics (Wave 4)

| Table | Dependencies |
|---|---|
| `activeforums_Replies` | TopicId **FK** → `activeforums_Topics.TopicId`; ContentId **FK** → `activeforums_Content.ContentId`; ReplyToId **logical** (self-ref) → `activeforums_Replies.ReplyId` |
| `activeforums_Poll` | TopicId **FK** → `activeforums_Topics.TopicId` |
| `activeforums_Topics_Tags` | TopicId **FK** → `activeforums_Topics.TopicId`; TagId **FK** → `activeforums_Tags.TagId` |
| `activeforums_Topics_Categories` | TopicId **FK** → `activeforums_Topics.TopicId`; CategoryId **FK** → `activeforums_Categories.CategoryId` |
| `activeforums_Topics_Ratings` | TopicId **FK** → `activeforums_Topics.TopicId` |
| `activeforums_Topics_Tracking` | TopicId **FK** → `activeforums_Topics.TopicId`; ForumId **logical** → `activeforums_Forums.ForumId`; LastReplyId **logical** → `activeforums_Replies.ReplyId` |
| `activeforums_Topics_Related` | SourceTopicId **logical** → `activeforums_Topics.TopicId`; RelatedTopicId **logical** → `activeforums_Topics.TopicId` |
| `activeforums_URL` | TopicId **FK** → `activeforums_Topics.TopicId`; ForumId **logical** → `activeforums_Forums.ForumId`; ForumGroupId **logical** → `activeforums_Groups.ForumGroupId` |
| `activeforums_Subscriptions` | TopicId **logical** → `activeforums_Topics.TopicId`; ForumId **logical** → `activeforums_Forums.ForumId` |

### Wave 6 — Depends on Topics + Forums + Replies (Waves 2, 4, 5)

| Table | Dependencies |
|---|---|
| `activeforums_ForumTopics` | TopicId **FK** → `activeforums_Topics.TopicId`; ForumId **FK** → `activeforums_Forums.ForumId`; LastReplyId **FK** (nullable) → `activeforums_Replies.ReplyId` |

### Wave 7 — Depends on Poll (Wave 5)

| Table | Dependencies |
|---|---|
| `activeforums_Poll_Options` | PollID **FK** → `activeforums_Poll.PollID` |
| `activeforums_Poll_Results` | PollID **FK** → `activeforums_Poll.PollID`; PollOptionID **logical** → `activeforums_Poll_Options.PollOptionsID` |

### Wave 8 — Depends on Attachments (Wave 4)

| Table | Dependencies |
|---|---|
| `activeforums_Content_Attachments` | ContentId **FK** → `activeforums_Content.ContentId`; AttachId **logical** → `activeforums_Attachments.AttachID` |

### Wave 9 — Depends on DNN Users/UserPortals

| Table | Dependencies |
|---|---|
| `activeforums_UserProfiles` | UserId + PortalId **logical** → DNN `UserPortals` |
| `activeforums_Forums_Tracking` | ForumId **logical** → `activeforums_Forums.ForumId`; UserId **logical** → DNN `Users` |
| `activeforums_AuditLog` | UserId **logical** → DNN `Users`; RelatedId is type-discriminated (no FK) |

### Wave 10 — Depends on Badges + UserProfiles (Waves 1, 9)

| Table | Dependencies |
|---|---|
| `activeforums_UserBadges` | BadgeId **FK** → `activeforums_Badges.BadgeId`; UserId **logical** → DNN `Users` |

### Operational / transient (not typically exported)

| Table | Notes |
|---|---|
| `activeforums_EmailNotificationQueue` | Pending email notifications; ModuleId/PortalId-scoped; transient |
| `activeforums_ProcessQueue` | Background processing queue; references Groups/Forums/Topics/Replies (logical); transient |

## Recommended export/import order for referential integrity

```text
Wave 1:  activeforums_Groups
         activeforums_Tags
         activeforums_Badges
         activeforums_Content
         activeforums_Permissions
         activeforums_Settings
         activeforums_Templates
         activeforums_Filters
         activeforums_Ranks
         activeforums_Properties
         activeforums_Security

Wave 2:  activeforums_Forums

Wave 3:  activeforums_Categories

Wave 4:  activeforums_Topics
         activeforums_Attachments
         activeforums_Likes
         activeforums_UserMentions

Wave 5:  activeforums_Replies
         activeforums_Poll
         activeforums_Topics_Tags
         activeforums_Topics_Categories
         activeforums_Topics_Ratings
         activeforums_Topics_Tracking
         activeforums_Topics_Related
         activeforums_URL
         activeforums_Subscriptions

Wave 6:  activeforums_ForumTopics

Wave 7:  activeforums_Poll_Options
         activeforums_Poll_Results

Wave 8:  activeforums_Content_Attachments

Wave 9:  activeforums_UserProfiles
         activeforums_Forums_Tracking
         activeforums_AuditLog

Wave 10: activeforums_UserBadges

Omit:    activeforums_Queue (standalone email outbox)
         activeforums_SearchCache (transient)
         activeforums_EmailNotificationQueue (transient)
         activeforums_ProcessQueue (transient)
```

## Key script references

- Initial core table + FK creation: `04.00.00.SqlDataProvider`
- Likes table: `06.01.03.SqlDataProvider`
- Content/topic/reply/tag/module cascade FK normalization: `07.00.07.SqlDataProvider`
- Reply/forum-topic FK corrections: `08.02.00.SqlDataProvider`
- User profile FK to `UserPortals`: `08.02.00.SqlDataProvider` / `08.02.04.SqlDataProvider`
- Categories / Topics_Categories tables: `09.01.00.SqlDataProvider`
- Badges / UserBadges tables: `09.02.00.SqlDataProvider`
- UserMentions table: `09.03.00.SqlDataProvider`
- Topics-tags FK refresh: `09.04.00.SqlDataProvider`
- Topics_Related / URL tables: `04.03.00.SqlDataProvider`
- EmailNotificationQueue / ProcessQueue tables: `09.06.01.SqlDataProvider` / `09.07.00.SqlDataProvider`
