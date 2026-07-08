CREATE TABLE "ToDoUser"
(
	"UserId" UUID NOT NULL PRIMARY KEY,
	"TelegramUserId" BIGINT,
	"TelegramUserName" VARCHAR(1024),
	"RegisteredAt" TIMESTAMP
);

CREATE TABLE "ToDoList"
(
	"Id" UUID NOT NULL PRIMARY KEY,
	"Name" VARCHAR(1024),
	"UserId" UUID,
	"CreatedAt" TIMESTAMP,
	CONSTRAINT "fk_UserId" FOREIGN KEY ("UserId")
		REFERENCES "ToDoUser" ("UserId")
);

CREATE TABLE "ToDoItem"
(
	"Id" UUID NOT NULL PRIMARY KEY,
	"UserId" UUID,
	"Name" VARCHAR(1024),
	"CreatedAt" TIMESTAMP,
	"State" INTEGER, -- 0 Active, 1 - Completed.
	"StateChangedAt" TIMESTAMP,
	"Deadline" TIMESTAMP,
	"ListId" UUID,
	CONSTRAINT "fk_UserId" FOREIGN KEY ("UserId")
		REFERENCES "ToDoUser" ("UserId"),
	CONSTRAINT "fk_ListId" FOREIGN KEY ("ListId")
		REFERENCES "ToDoList" ("Id")
);

CREATE INDEX "UX_ToDoList_UserId" ON "ToDoList"("UserId");
CREATE INDEX "UX_ToDoItem_UserId" ON "ToDoItem"("UserId");
CREATE INDEX "UX_ToDoItem_ListId" ON "ToDoItem"("ListId");
CREATE INDEX "UX_ToDoUser_TelegramUserId" ON "ToDoUser"("TelegramUserId");