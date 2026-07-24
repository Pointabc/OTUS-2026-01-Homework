CREATE TABLE "Notification"
(
	"Id" UUID NOT NULL PRIMARY KEY,
	"UserId" UUID,
	"Type" VARCHAR(1024),
	"Text" VARCHAR(1024),
	"ScheduledAt" TIMESTAMP,
	"IsNotified" BOOL,
	"NotifiedAt" TIMESTAMP,
	CONSTRAINT "fk_UserId" FOREIGN KEY ("UserId")
		REFERENCES "ToDoUser" ("UserId")
);

CREATE INDEX "UX_Notification_UserId" ON "Notification"("UserId");