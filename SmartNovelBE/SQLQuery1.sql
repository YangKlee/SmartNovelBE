-- Tạo Database
CREATE DATABASE SmartTruyenDB;
GO

USE SmartTruyenDB
GO

-- ====================================================================================
-- 1. TẠO CÁC BẢNG ĐỘC LẬP (Không chứa khóa ngoại)
-- ====================================================================================

CREATE TABLE [Role] (
    RoleID VARCHAR(36) NOT NULL,
    RoleDisplayName NVARCHAR(255) NOT NULL,
    RoleDescription NVARCHAR(1000),
    CONSTRAINT PK_Role PRIMARY KEY (RoleID)
);

CREATE TABLE Category (
    CategoryID VARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000),
    Slug VARCHAR(255) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT PK_Category PRIMARY KEY (CategoryID)
);

-- ====================================================================================
-- 2. TẠO BẢNG CẤP 1 (Phụ thuộc bảng Độc lập)
-- ====================================================================================

CREATE TABLE [User] (
    UID VARCHAR(36) NOT NULL,
    Username VARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Birthday DATE NULL, -- Bổ sung trường ngày sinh
    Password VARCHAR(255) NOT NULL,
    RoleID VARCHAR(36) NOT NULL,
    AvartarUrl VARCHAR(500),
    Phone VARCHAR(20) UNIQUE,
    Status VARCHAR(20) NOT NULL,
    BannedTime DATETIME NULL,
    TimeOutTime DATETIME NULL,
    TimeOutType VARCHAR(20) NULL,
    CreatorPoint INT DEFAULT 0,
    ReadingTheme VARCHAR(20) DEFAULT 'light',
    ReadingFontSize INT DEFAULT 19,
    ReadingFontFamily VARCHAR(50) DEFAULT 'Roboto',
    CONSTRAINT PK_User PRIMARY KEY (UID),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES [Role](RoleID) ON DELETE NO ACTION
);

-- ====================================================================================
-- 3. TẠO BẢNG CẤP 2 (Phụ thuộc User, Category)
-- ====================================================================================

CREATE TABLE Novel (
    NovelID VARCHAR(36) NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Slug VARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    AgeRating VARCHAR(20) NOT NULL,
    ImageNovelUrl VARCHAR(500),
    ImageBanerNovelUrl VARCHAR(500),
    Status VARCHAR(20) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    ViewCount INT DEFAULT 0,
    LikeCount INT DEFAULT 0,
    CreateTime DATETIME DEFAULT GETDATE(),
    UpdateTime DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Novel PRIMARY KEY (NovelID),
    CONSTRAINT FK_Novel_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

CREATE TABLE FollowAuthor (
    FollowerUID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    CONSTRAINT PK_FollowAuthor PRIMARY KEY (FollowerUID, UID),
    CONSTRAINT FK_FollowAuthor_Target FOREIGN KEY (FollowerUID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_FollowAuthor_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

-- BẢNG MỚI: Người dùng chặn Tác giả
CREATE TABLE BlockAuthor (
    AuthorID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    CONSTRAINT PK_BlockAuthor PRIMARY KEY (AuthorID, UID),
    CONSTRAINT FK_BlockAuthor_Target FOREIGN KEY (AuthorID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_BlockAuthor_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

-- BẢNG MỚI: Người dùng chặn Thể loại
CREATE TABLE BlockCategory (
    CategoryID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    CONSTRAINT PK_BlockCategory PRIMARY KEY (CategoryID, UID),
    CONSTRAINT FK_BlockCategory_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID) ON DELETE CASCADE,
    CONSTRAINT FK_BlockCategory_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

-- ====================================================================================
-- 4. TẠO BẢNG CẤP 3 (Phụ thuộc Novel, Category, User)
-- ====================================================================================

CREATE TABLE NovelCategory (
    NovelID VARCHAR(36) NOT NULL,
    CategoryID VARCHAR(36) NOT NULL,
    CONSTRAINT PK_NovelCategory PRIMARY KEY (NovelID, CategoryID),
    CONSTRAINT FK_NovelCategory_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE CASCADE,
    CONSTRAINT FK_NovelCategory_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID) ON DELETE CASCADE
);

CREATE TABLE Chapter (
    ChapterID VARCHAR(36) NOT NULL,
    ChaperOrder INT NOT NULL,
    NovelID VARCHAR(36) NOT NULL,
    ChapterTitle NVARCHAR(255) NOT NULL,
    SummaryChapter NVARCHAR(1000),
    ChapterFileUrl VARCHAR(500),
    Status VARCHAR(20) NOT NULL,
    CreateTime DATETIME DEFAULT GETDATE(),
    UpdateTime DATETIME DEFAULT GETDATE(),
    AllowComment BIT DEFAULT 1,
    CONSTRAINT PK_Chapter PRIMARY KEY (ChapterID),
    CONSTRAINT FK_Chapter_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE CASCADE
);

CREATE TABLE Rating (
    UID VARCHAR(36) NOT NULL,
    NovelID VARCHAR(36) NOT NULL,
    RatingPoint FLOAT NOT NULL,
    CONSTRAINT PK_Rating PRIMARY KEY (UID, NovelID),
    CONSTRAINT FK_Rating_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_Rating_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE CASCADE
);

CREATE TABLE FollowNovel (
    NovelID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    CONSTRAINT PK_FollowNovel PRIMARY KEY (NovelID, UID),
    CONSTRAINT FK_FollowNovel_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE CASCADE,
    CONSTRAINT FK_FollowNovel_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

CREATE TABLE RecommendNovel (
    RecommendID VARCHAR(36) NOT NULL,
    NovelID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    CONSTRAINT PK_RecommendNovel PRIMARY KEY (RecommendID),
    CONSTRAINT FK_RecommendNovel_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE CASCADE,
    CONSTRAINT FK_RecommendNovel_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION
);

-- ====================================================================================
-- 5. TẠO BẢNG CẤP 4 (Phụ thuộc Chapter, Comment, v.v.)
-- ====================================================================================

CREATE TABLE Comment (
    CommentID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    ChapterID VARCHAR(36) NOT NULL,
    ParentCommentID VARCHAR(36) NULL,
    TimeCommeny DATETIME DEFAULT GETDATE(),
    Content NVARCHAR(MAX) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT PK_Comment PRIMARY KEY (CommentID),
    CONSTRAINT FK_Comment_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_Comment_Chapter FOREIGN KEY (ChapterID) REFERENCES Chapter(ChapterID) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_Parent FOREIGN KEY (ParentCommentID) REFERENCES Comment(CommentID) ON DELETE NO ACTION
);

CREATE TABLE HistoryReader (
    ReadSessionID VARCHAR(36) NOT NULL,
    UID VARCHAR(36) NOT NULL,
    NovelID VARCHAR(36) NOT NULL,
    ChapterID VARCHAR(36) NOT NULL,
    TimeReader DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_HistoryReader PRIMARY KEY (ReadSessionID),
    CONSTRAINT FK_HistoryReader_User FOREIGN KEY (UID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_HistoryReader_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE NO ACTION,
    CONSTRAINT FK_HistoryReader_Chapter FOREIGN KEY (ChapterID) REFERENCES Chapter(ChapterID) ON DELETE CASCADE
);

CREATE TABLE ReportTicket (
    TiketID VARCHAR(36) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    ReasonDetail NVARCHAR(MAX) NOT NULL,
    NovelID VARCHAR(36) NULL,
    ChapterID VARCHAR(36) NULL,
    CommentID VARCHAR(36) NULL,
    TargetUID VARCHAR(36) NULL,
    RepoterUID VARCHAR(36) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    ResolvedUID VARCHAR(36) NULL,
    TimeSend DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_ReportTicket PRIMARY KEY (TiketID),
    CONSTRAINT FK_ReportTicket_Novel FOREIGN KEY (NovelID) REFERENCES Novel(NovelID) ON DELETE NO ACTION,
    CONSTRAINT FK_ReportTicket_Chapter FOREIGN KEY (ChapterID) REFERENCES Chapter(ChapterID) ON DELETE NO ACTION,
    CONSTRAINT FK_ReportTicket_Comment FOREIGN KEY (CommentID) REFERENCES Comment(CommentID) ON DELETE NO ACTION,
    CONSTRAINT FK_ReportTicket_TargetUser FOREIGN KEY (TargetUID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_ReportTicket_RepoterUser FOREIGN KEY (RepoterUID) REFERENCES [User](UID) ON DELETE NO ACTION,
    CONSTRAINT FK_ReportTicket_ResolvedUser FOREIGN KEY (ResolvedUID) REFERENCES [User](UID) ON DELETE NO ACTION
);
GO

CREATE TABLE MenuNav (
    ID INT IDENTITY(1,1) NOT NULL,
    RoleID VARCHAR(36) NOT NULL,
    IconBootstrap VARCHAR(100),
    Content NVARCHAR(255) NOT NULL,
    ParentID INT NULL,
    UrlLink VARCHAR(500),

    CONSTRAINT PK_MenuNav PRIMARY KEY (ID),

    -- Khóa ngoại tới bảng Role
    CONSTRAINT FK_MenuNav_Role 
        FOREIGN KEY (RoleID) 
        REFERENCES [Role](RoleID)
        ON DELETE CASCADE,

    -- Tự tham chiếu tới chính nó (menu cha)
    CONSTRAINT FK_MenuNav_Parent 
        FOREIGN KEY (ParentID) 
        REFERENCES MenuNav(ID)
        ON DELETE NO ACTION
);
ALTER TABLE [MenuNav]
ADD Slots INT DEFAULT(0)
INSERT INTO [Role] (RoleID, RoleDisplayName, RoleDescription)
VALUES
('1', N'Admin', N'Quản trị viên hệ thống'),
('2', N'Moderator', N'Kiểm duyệt viên'),
('3', N'Author', N'Tác giả'),
('4', N'Reader', N'Độc giả');

-- INSERT USER TEST
INSERT INTO [User]
(
    UID,
    Username,
    DisplayName,
    Email,
    Birthday,
    Password,
    RoleID,
    AvartarUrl,
    Phone,
    Status,
    BannedTime,
    TimeOutTime,
    TimeOutType,
    CreatorPoint
)
VALUES
-- Admin
(
    'U001',
    'admin01',
    N'Admin Chính',
    'admin01@gmail.com',
    '2000-01-01',
    '123456',
    '1',
    'https://i.pravatar.cc/300?img=1',
    '0900000001',
    'ACTIVE',
    NULL,
    NULL,
    NULL,
    1000
),

-- Moderator
(
    'U002',
    'mod01',
    N'Mod Kiểm Duyệt',
    'mod01@gmail.com',
    '2001-02-02',
    '123456',
    '2',
    'https://i.pravatar.cc/300?img=2',
    '0900000002',
    'ACTIVE',
    NULL,
    NULL,
    NULL,
    500
),

-- Author
(
    'U003',
    'author01',
    N'Tác Giả A',
    'author01@gmail.com',
    '2002-03-03',
    '123456',
    '3',
    'https://i.pravatar.cc/300?img=3',
    '0900000003',
    'ACTIVE',
    NULL,
    NULL,
    NULL,
    250
),

-- Reader 1
(
    'U004',
    'reader01',
    N'Độc Giả 1',
    'reader01@gmail.com',
    '2003-04-04',
    '123456',
    '4',
    'https://i.pravatar.cc/300?img=4',
    '0900000004',
    'ACTIVE',
    NULL,
    NULL,
    NULL,
    0
),

-- Reader 2
(
    'U005',
    'reader02',
    N'Độc Giả 2',
    'reader02@gmail.com',
    '2004-05-05',
    '123456',
    '4',
    'https://i.pravatar.cc/300?img=5',
    '0900000005',
    'BANNED',
    GETDATE(),
    NULL,
    NULL,
    0
);

select * from [User]