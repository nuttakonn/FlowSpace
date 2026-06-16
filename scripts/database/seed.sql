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
)
ON CONFLICT (id) DO NOTHING;
