# Template Marketplace Architecture

## Overview
The Template Marketplace is a global ecosystem within FlowSpace where users can discover, share, and rate high-quality visual blueprints. It extends the core Template System to support community-driven growth and collaborative architecting.

---

## 1. Marketplace Model

### Visibility Tiers
| Tier | Description | Access |
| :--- | :--- | :--- |
| **System** | Official templates provided by FlowSpace. | Global (Read-only) |
| **Public** | User-contributed templates published to the marketplace. | Global (Read-only) |
| **Private** | Personal blueprints or drafts. | Creator Only |

---

## 2. Entities

### BoardTemplate (Extended)
| Property | Type | Description |
| :--- | :--- | :--- |
| **Visibility** | Enum | `Private`, `Public`, `System`. |
| **Version** | String | Semver (e.g., "1.0.0"). |
| **ParentTemplateId** | UUID | For tracking forks/clones from the marketplace. |
| **Tags** | List<String> | Searchable labels (e.g., "AWS", "Agile"). |
| **UsageCount** | Integer | Number of times the template has been cloned or used. |
| **AverageRating** | Decimal | Calculated from user reviews. |

### TemplateRating
| Property | Type | Description |
| :--- | :--- | :--- |
| **Id** | UUID | Unique identifier. |
| **TemplateId** | UUID | Reference to the `BoardTemplate`. |
| **UserId** | UUID | Reference to the `User`. |
| **Score** | Integer | 1 to 5 stars. |
| **Comment** | String | Optional feedback. |
| **CreatedAt** | DateTime | Timestamp. |

---

## 3. Core Workflows

### Publishing a Template
1. **Selection**: User selects a `Private` template they created.
2. **Metadata**: User adds tags, a long-form description, and confirms the `BoardType`.
3. **Screenshot**: The **Export Worker** generates a clean PNG thumbnail.
4. **Transition**: Visibility is set to `Public`. The template is now searchable in the Marketplace.

### Cloning a Marketplace Template
1. **Discovery**: User finds a `Public` or `System` template.
2. **Clone Action**: 
    - A deep copy of the template record is created.
    - Visibility is set to `Private` for the current user.
    - `ParentTemplateId` is set to the original marketplace ID.
    - Global `UsageCount` of the parent is incremented.
3. **Result**: The user now has a personal version they can modify or use to create new boards.

### Template Versioning
Marketplace templates are immutable once published to a specific version.
1. **Update**: To improve a public template, the creator makes changes to their local copy.
2. **Republish**: Creator clicks "Publish Update".
3. **Version Bump**: A new `BoardTemplate` record is created with the same name but an incremented `Version` (e.g., 1.1.0).
4. **Lineage**: The marketplace UI shows the "Latest" version by default but allows accessing "Legacy" versions.

---

## 4. API Specification

### Marketplace Discovery
`GET /api/v1/marketplace/templates`
- **Params**: `?query=...&tag=...&sort=[rating|recent|popular]`
- **Response**: `List<MarketplaceTemplateResponse>`

### Template Interaction
`POST /api/v1/marketplace/templates/{id}/publish`
- **Request**: `{ "version": "1.0.0", "tags": ["AWS"] }`
- **Action**: Converts Private -> Public.

`POST /api/v1/marketplace/templates/{id}/clone`
- **Action**: Creates a private copy for the user.

`POST /api/v1/marketplace/templates/{id}/rate`
- **Request**: `{ "score": 5, "comment": "Great structure!" }`
- **Action**: Persists rating and updates parent `AverageRating`.

---

## 5. Community & Trust

### Moderation
- **Report**: Users can report malicious or inappropriate templates.
- **Verification**: Templates can be "Verified" (FlowSpace Blue Check) to indicate high quality or official partnership.

### Analytics
- **Download Tracking**: Tracks which industries (tags) are most active.
- **Conversion Rate**: Ratio of "View" to "Clone" to identify the most effective blueprints.

---

## 6. UI Integration

### Marketplace Dashboard
A dedicated "Explore" tab in the main application.
- **Hero Section**: Featured/Verified templates.
- **Trending**: High `UsageCount` in the last 7 days.
- **My Contributions**: List of templates the user has published.

### Rating UI
Prompted to the user 24 hours after they first instantiate a board from a marketplace template.
