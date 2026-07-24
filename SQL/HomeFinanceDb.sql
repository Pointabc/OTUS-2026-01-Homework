-- Тип прихода.
CREATE TABLE "IncomeType"
(
	"IncomeTypeId" UUID NOT NULL PRIMARY KEY,
	"Name" VARCHAR(1024),
	"CreatedAt" TIMESTAMP
);

-- Тип расхода.
CREATE TABLE "ExpenseType"
(
	"ExpenseTypeId" UUID NOT NULL PRIMARY KEY,
	"Name" VARCHAR(1024),
	"CreatedAt" TIMESTAMP
);

-- Пользователь Telegram бота.
CREATE TABLE "FinanceUser"
(
	"FinanceUserId" UUID NOT NULL PRIMARY KEY,
	"TelegramUserId" BIGINT,
	"TelegramUserName" VARCHAR(1024),
	"RegisteredAt" TIMESTAMP
);

-- Приход.
CREATE TABLE "Income"
(
	"IncomeId" UUID NOT NULL PRIMARY KEY,
	"IncomeTypeId" UUID,
	"Amount" NUMERIC(10, 2),
	"Note" VARCHAR(1024),
	"FinanceUserId" UUID,
	"CreatedAt" TIMESTAMP,
	CONSTRAINT "fk_FinanceUserId" FOREIGN KEY ("FinanceUserId")
		REFERENCES "FinanceUser" ("FinanceUserId"),
	CONSTRAINT "fk_IncomeTypeId" FOREIGN KEY ("IncomeTypeId")
		REFERENCES "IncomeType" ("IncomeTypeId")
);

-- Расход.
CREATE TABLE "Expense"
(
	"ExpenseId" UUID NOT NULL PRIMARY KEY,
	"ExpenseTypeId" UUID,
	"Amount" NUMERIC(10, 2),
	"Note" VARCHAR(1024),
	"FinanceUserId" UUID,
	"CreatedAt" TIMESTAMP,
	CONSTRAINT "fk_FinanceUserId" FOREIGN KEY ("FinanceUserId")
		REFERENCES "FinanceUser" ("FinanceUserId"),
	CONSTRAINT "fk_ExpenseTypeId" FOREIGN KEY ("ExpenseTypeId")
		REFERENCES "ExpenseType" ("ExpenseTypeId")
);

CREATE INDEX "UX_Expense_ExpenseTypeId" ON "Expense"("ExpenseTypeId");
CREATE INDEX "UX_Expense_FinanceUserId" ON "Expense"("FinanceUserId");
CREATE INDEX "UX_Income_IncomeTypeId" ON "Income"("IncomeTypeId");
CREATE INDEX "UX_Income_FinanceUserId" ON "Income"("FinanceUserId");