CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `User` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `GoogleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `ProfilePicture` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `EducationLevel` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Role` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `PlanType` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `CreatedAt` datetime(6) NULL,
        `LastLoginAt` datetime(6) NULL,
        `LastActivityAt` datetime(6) NULL,
        `IsActive` tinyint(1) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Subject` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Color` varchar(7) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Icon` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `OrderIndex` int NULL,
        `UserId` int NOT NULL,
        `CreatedAt` datetime(6) NULL,
        `IsArchived` tinyint(1) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Subject_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Document` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FileName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `OriginalFileName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `FileType` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `FileUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `ExtractedText` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `FileSizeKb` int NULL,
        `PageCount` int NULL,
        `Language` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `ProcessingStatus` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `ProcessingError` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `SubjectId` int NOT NULL,
        `UploadedAt` datetime(6) NULL,
        `ProcessedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Document_Subject_SubjectId` FOREIGN KEY (`SubjectId`) REFERENCES `Subject` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `ConceptMap` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `NodesJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `EdgesJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `ImageUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `NodeCount` int NULL,
        `EdgeCount` int NULL,
        `JsonSchemaVersion` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `AiModelUsed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `DocumentId` int NOT NULL,
        `GeneratedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ConceptMap_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Flashcard` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Question` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Answer` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Difficulty` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `NextReviewDate` datetime(6) NULL,
        `ReviewCount` int NULL,
        `EaseFactor` float NULL,
        `IntervalDays` int NULL,
        `IsManuallyEdited` tinyint(1) NULL,
        `DocumentId` int NOT NULL,
        `CreatedAt` datetime(6) NULL,
        `LastReviewedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Flashcard_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `MindMap` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `NodesJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `ImageUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `ColorScheme` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `NodeCount` int NULL,
        `JsonSchemaVersion` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `AiModelUsed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `DocumentId` int NOT NULL,
        `GeneratedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MindMap_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Quiz` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `TotalQuestions` int NOT NULL,
        `DocumentId` int NOT NULL,
        `GeneratedAt` datetime(6) NULL,
        `IsActive` tinyint(1) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Quiz_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `StudySession` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `DocumentId` int NULL,
        `ActivityType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `DurationMinutes` int NULL,
        `StartedAt` datetime(6) NULL,
        `EndedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StudySession_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_StudySession_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Summary` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `Content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `WordCount` int NULL,
        `IsFavorite` tinyint(1) NULL,
        `RegenerationCount` int NULL,
        `AiModelUsed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `DocumentId` int NOT NULL,
        `GeneratedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Summary_Document_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Document` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `FlashcardReview` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FlashcardId` int NOT NULL,
        `UserId` int NOT NULL,
        `Rating` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `ReviewedAt` datetime(6) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FlashcardReview_Flashcard_FlashcardId` FOREIGN KEY (`FlashcardId`) REFERENCES `Flashcard` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_FlashcardReview_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `Question` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `QuestionText` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `QuestionType` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `CorrectAnswer` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `OptionsJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Explanation` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `Points` int NULL,
        `OrderIndex` int NOT NULL,
        `QuizId` int NOT NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Question_Quiz_QuizId` FOREIGN KEY (`QuizId`) REFERENCES `Quiz` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE TABLE `QuizAttempt` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `QuizId` int NOT NULL,
        `UserId` int NOT NULL,
        `Score` int NOT NULL,
        `CorrectAnswers` int NOT NULL,
        `TotalQuestions` int NOT NULL,
        `AnswersJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `AttemptNumber` int NOT NULL,
        `TimeSpentSeconds` int NULL,
        `StartedAt` datetime(6) NOT NULL,
        `CompletedAt` datetime(6) NULL,
        `IsLatest` tinyint(1) NULL,
        CONSTRAINT `PRIMARY` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_QuizAttempt_Quiz_QuizId` FOREIGN KEY (`QuizId`) REFERENCES `Quiz` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_QuizAttempt_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_conceptmaps_DocumentId` ON `ConceptMap` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_documents_SubjectId` ON `Document` (`SubjectId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_flashcards_DocumentId` ON `Flashcard` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_flashcardreviews_FlashcardId` ON `FlashcardReview` (`FlashcardId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_flashcardreviews_UserId` ON `FlashcardReview` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_mindmaps_DocumentId` ON `MindMap` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_questions_QuizId` ON `Question` (`QuizId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_quizzes_DocumentId` ON `Quiz` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_quizattempts_QuizId` ON `QuizAttempt` (`QuizId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_quizattempts_UserId` ON `QuizAttempt` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_studysessions_DocumentId` ON `StudySession` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_studysessions_UserId` ON `StudySession` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_subjects_UserId` ON `Subject` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE INDEX `IX_summaries_DocumentId` ON `Summary` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE UNIQUE INDEX `Email` ON `User` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    CREATE UNIQUE INDEX `GoogleId` ON `User` (`GoogleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251130003428_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251130003428_InitialCreate', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

