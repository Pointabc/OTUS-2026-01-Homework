-- ****************************************************************************************************
--Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
SELECT "Name" as TaskName, "CreatedAt", "State" FROM "ToDoItem"
WHERE "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11';
-- ****************************************************************************************************
-- ****************************************************************************************************
--//Возвращает ToDoItems для UserId со статусом Active
--Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);
SELECT "Name" as TaskName, "CreatedAt" FROM "ToDoItem"
WHERE "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11' AND "ToDoItem"."State" = 1;
-- ****************************************************************************************************
-- ****************************************************************************************************
-- Получить задачу по Guid
--Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken);
SELECT "Name" as TaskName, "CreatedAt", "State" FROM "ToDoItem"
WHERE "Id" = 'aabbcc11-3a4a-5b5b-9a8f-7aa9bd380a11';
-- ****************************************************************************************************
-- ****************************************************************************************************
--Task Add(ToDoItem item, CancellationToken cancellationToken);
INSERT INTO "ToDoItem" (
    "Id",
    "UserId",
    "Name",
    "CreatedAt",
    "State",
    "StateChangedAt",
    "Deadline",
    "ListId"
) VALUES (
    gen_random_uuid(),  -- автоматическая генерация UUID
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Новая задача',
    NOW(),
    0,
    NULL,
    '2024-12-31 23:59:59',
    'f5aabb66-8a9c-4e0d-9f3a-2ff9bd380a11'
); 
-- ****************************************************************************************************
-- ****************************************************************************************************
-- Обновить всю запись.
--Task Update(ToDoItem item, CancellationToken cancellationToken);
UPDATE "ToDoItem" 
SET 
    "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    "Name" = 'Новое название задачи',
    "CreatedAt" = '2024-01-16 10:00:00',
    "State" = 1,
    "StateChangedAt" = NOW(),
    "Deadline" = '2024-01-20 17:00:00',
    "ListId" = 'f5aabb66-8a9c-4e0d-9f3a-2ff9bd380a11'
WHERE "Id" = 'aabbcc77-9a0a-1a1a-9a4a-3aa9bd380a77';
-- ****************************************************************************************************
-- ****************************************************************************************************
--Task Delete(Guid id, CancellationToken cancellationToken);
DELETE FROM "ToDoItem" 
WHERE "Id" = 'aabbcc11-3a4a-5b5b-9a8f-7aa9bd380a11';
-- ****************************************************************************************************
-- ****************************************************************************************************
--//Проверяет есть ли задача с таким именем у пользователя
--Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken);
SELECT EXISTS(
    SELECT 1 
    FROM "ToDoItem" 
    WHERE "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11' 
      AND "Name" = 'Провести встречу с командой'
) AS "Exists";
-- ****************************************************************************************************
-- ****************************************************************************************************
--//Возвращает количество активных задач у пользователя
--Task<int> CountActive(Guid userId, CancellationToken cancellationToken);
SELECT count("Id") FROM "ToDoItem"
WHERE "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11' AND "State" = 0
-- ****************************************************************************************************
-- ****************************************************************************************************
--Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
SELECT * 
FROM "ToDoItem"
WHERE "UserId" = 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
  AND "Name" ILIKE 'Новая%';
-- ****************************************************************************************************