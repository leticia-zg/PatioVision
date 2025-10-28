DECLARE
V_COUNT INTEGER;
BEGIN
SELECT COUNT(TABLE_NAME) INTO V_COUNT from USER_TABLES where TABLE_NAME = '__EFMigrationsHistory';
IF V_COUNT = 0 THEN
Begin
BEGIN 
EXECUTE IMMEDIATE 'CREATE TABLE 
"__EFMigrationsHistory" (
    "MigrationId" NVARCHAR2(150) NOT NULL,
    "ProductVersion" NVARCHAR2(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
)';
END;

End;

END IF;
EXCEPTION
WHEN OTHERS THEN
    IF(SQLCODE != -942)THEN
        RAISE;
    END IF;
END;
/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "DISPOSITIVO_IOT" (
        "DispositivoIotId" RAW(16) NOT NULL,
        "Tipo" NVARCHAR2(2000) NOT NULL,
        "UltimaLocalizacao" NVARCHAR2(255),
        "UltimaAtualizacao" TIMESTAMP(7) NOT NULL,
        "DtCadastro" TIMESTAMP(7) NOT NULL,
        "DtAtualizacao" TIMESTAMP(7),
        CONSTRAINT "PK_DISPOSITIVO_IOT" PRIMARY KEY ("DispositivoIotId")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "USUARIO" (
        "Id" RAW(16) NOT NULL,
        "Nome" NVARCHAR2(2000) NOT NULL,
        "Email" NVARCHAR2(2000) NOT NULL,
        "Senha" NVARCHAR2(2000) NOT NULL,
        "Perfil" RAW(16) NOT NULL,
        "DtCriacao" TIMESTAMP(7) NOT NULL,
        "DtAlteracao" TIMESTAMP(7) NOT NULL,
        "Ativo" NUMBER(1) NOT NULL,
        CONSTRAINT "PK_USUARIO" PRIMARY KEY ("Id")
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "PATIO" (
        "PatioId" RAW(16) NOT NULL,
        "Nome" NVARCHAR2(100) NOT NULL,
        "Categoria" NVARCHAR2(2000) NOT NULL,
        "Latitude" DECIMAL(18,10) NOT NULL,
        "Longitude" DECIMAL(18,10) NOT NULL,
        "DispositivoIotId" RAW(16) NOT NULL,
        "DtCadastro" TIMESTAMP(7) NOT NULL,
        "DtAtualizacao" TIMESTAMP(7),
        CONSTRAINT "PK_PATIO" PRIMARY KEY ("PatioId"),
        CONSTRAINT "FK_PATIO_DISPOSITIVO_IOT_DispositivoIotId" FOREIGN KEY ("DispositivoIotId") REFERENCES "DISPOSITIVO_IOT" ("DispositivoIotId") ON DELETE CASCADE
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    BEGIN 
    EXECUTE IMMEDIATE 'CREATE TABLE 
    "MOTO" (
        "MotoId" RAW(16) NOT NULL,
        "Modelo" NVARCHAR2(50) NOT NULL,
        "Placa" NVARCHAR2(10),
        "Status" NVARCHAR2(2000) NOT NULL,
        "PatioId" RAW(16) NOT NULL,
        "DispositivoIotId" RAW(16) NOT NULL,
        "DtCadastro" TIMESTAMP(7) NOT NULL,
        "DtAtualizacao" TIMESTAMP(7),
        CONSTRAINT "PK_MOTO" PRIMARY KEY ("MotoId"),
        CONSTRAINT "FK_MOTO_DISPOSITIVO_IOT_DispositivoIotId" FOREIGN KEY ("DispositivoIotId") REFERENCES "DISPOSITIVO_IOT" ("DispositivoIotId") ON DELETE CASCADE,
        CONSTRAINT "FK_MOTO_PATIO_PatioId" FOREIGN KEY ("PatioId") REFERENCES "PATIO" ("PatioId") ON DELETE CASCADE
    )';
    END;
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE '
    CREATE INDEX "IX_MOTO_DispositivoIotId" ON "MOTO" ("DispositivoIotId")
    ';
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE '
    CREATE INDEX "IX_MOTO_PatioId" ON "MOTO" ("PatioId")
    ';
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE '
    CREATE INDEX "IX_PATIO_DispositivoIotId" ON "PATIO" ("DispositivoIotId")
    ';
 END IF;
END;

/

DECLARE
    v_Count INTEGER;
BEGIN
SELECT COUNT(*) INTO v_Count FROM "__EFMigrationsHistory" WHERE "MigrationId" = N'20251028003535_InitialCreate';
IF v_Count = 0 THEN

    EXECUTE IMMEDIATE '
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES (N''20251028003535_InitialCreate'', N''9.0.5'')
    ';
 END IF;
END;

/

