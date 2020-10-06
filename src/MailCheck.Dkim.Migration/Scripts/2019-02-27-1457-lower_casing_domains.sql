SET SQL_SAFE_UPDATES=0;
  
UPDATE dkim_entity
SET 
id = LOWER(id),
state = JSON_SET(state, '$.id', LOWER(state->>'$.id'))
WHERE id REGEXP BINARY '[A-Z]';

UPDATE dkim_entity_history
SET 
id = LOWER(id),
state = JSON_SET(state, '$.id', LOWER(state->>'$.id'))
WHERE id REGEXP BINARY '[A-Z]';

SET SQL_SAFE_UPDATES=1;