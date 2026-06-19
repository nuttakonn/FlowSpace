-- Seed System Templates
INSERT INTO board_templates (id, name, description, board_type, content_json, is_system, created_at)
VALUES 
(
    '018f4a1a-4a1a-7000-8000-000000000001', 
    'Software Architecture', 
    'Standard blueprint for microservices and cloud deployments.', 
    'Flowchart', 
    '{"nodes": [], "edges": []}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000002', 
    'Mindmap Canvas', 
    'Creative space for brainstorming and idea mapping.', 
    'Mindmap', 
    '{"nodes": [], "edges": []}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000003', 
    'Agile Whiteboard', 
    'Empty space optimized for sprint planning and retro.', 
    'Whiteboard', 
    '{}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000004', 
    'UML Class Diagram', 
    'Model software classes, attributes, and relationships.', 
    'Flowchart', 
    '{}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000005', 
    'Kanban Board', 
    'Track task status and project progress visually.', 
    'Whiteboard', 
    '{}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000006', 
    'User Journey Map', 
    'Map user steps, touchpoints, and emotions.', 
    'Whiteboard', 
    '{}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000007', 
    'Concept Mindmap', 
    'Structure thoughts and brainstorm central concepts.', 
    'Mindmap', 
    '{}', 
    true, 
    NOW()
),
(
    '018f4a1a-4a1a-7000-8000-000000000008', 
    'Project Timeline', 
    'Sequence deliverables and milestones left-to-right.', 
    'Flowchart', 
    '{}', 
    true, 
    NOW()
)
ON CONFLICT (id) DO NOTHING;
