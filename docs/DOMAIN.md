# Domain Model

## User

Represents a platform user.

Properties:

* Id
* Email
* DisplayName
* AvatarUrl

Relationships:

* Owns Workspaces
* Participates in Workspaces

---

## Workspace

Container for boards.

Properties:

* Id
* Name
* OwnerId

Relationships:

* Contains Boards
* Contains Members

---

## Board

Visual canvas.

Properties:

* Id
* WorkspaceId
* Name
* Type

Board Types:

* Whiteboard
* Flowchart
* Mindmap
* Wireframe

---

## Node

Visual element placed on board.

Properties:

* Id
* BoardId
* Type
* Position
* Size
* Metadata

Node Types:

* Rectangle
* Circle
* Diamond
* Text
* StickyNote
* Image
* WireframeComponent

---

## Edge

Connection between nodes.

Properties:

* Id
* BoardId
* SourceNodeId
* TargetNodeId

---

## BoardPermission

Defines access rights.

Roles:

* Owner
* Editor
* Viewer

---

## CollaborationSession

Represents active realtime editing session.

Properties:

* BoardId
* UserId
* ConnectedAt

---

## AIGenerationRequest

Stores AI generation history.

Properties:

* Id
* UserId
* Prompt
* Output
* CreatedAt
