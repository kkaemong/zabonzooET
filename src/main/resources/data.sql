-- Stage data for local development
INSERT INTO stage (stage_id, world_no, stage_no, stage_code, stage_name, stage_order, stage_length, base_reward_coin, is_active)
VALUES 
(1, 1, 1, 'ERA_1980', '1980년대', 1, 700, 100, true),
(2, 1, 2, 'ERA_2000', '2000년대', 2, 700, 100, true),
(3, 1, 3, 'ERA_2020', '2020년대', 3, 700, 100, true)
ON CONFLICT (stage_id) DO NOTHING;
