-- 1) Crie o banco (ajuste o nome se mudar em appsettings.json)
CREATE DATABASE IF NOT EXISTS SistemaBancarioDB
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE SistemaBancarioDB;

-- 2) Tabelas equivalentes ao modelo EF Core (Pomelo / MySQL 8)

DROP TABLE IF EXISTS `Transactions`;
DROP TABLE IF EXISTS `Accounts`;

CREATE TABLE `Accounts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
  `Email` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
  `PasswordHash` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
  `Balance` decimal(18,2) NOT NULL,
  `Role` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `IX_Accounts_Email` (`Email`)
) ENGINE=InnoDB;

CREATE TABLE `Transactions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `AccountId` int NOT NULL,
  `Type` int NOT NULL COMMENT '0=Deposit, 1=Withdraw, 2=Transfer (valor negativo=saida, positivo=entrada)',
  `Amount` decimal(18,2) NOT NULL,
  `Date` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_Transactions_AccountId` (`AccountId`),
  CONSTRAINT `FK_Transactions_Accounts_AccountId`
    FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Não insira usuários manualmente com hash inventado: use POST /api/auth/register
-- Para um administrador (lista todas as contas no GET /api/accounts), após cadastrar pelo app:
-- UPDATE Accounts SET Role = 'Administrador' WHERE Email = 'seu@email.com';
