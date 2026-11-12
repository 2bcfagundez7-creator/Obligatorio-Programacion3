-- ESQUEMA SQL SERVER - alternativa a las migraciones de Entity Framework 10.
-- En un proyecto local ya migrado NO hace falta ejecutar este archivo.
-- Para una base vacía: ejecutar este archivo y luego DatosIniciales.sql.
-- Si se usa este script manual, no ejecutar después `dotnet ef database update` sobre esa misma base.
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Usuarios',N'U') IS NULL
CREATE TABLE dbo.Usuarios(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreCompleto nvarchar(max) NOT NULL, Email nvarchar(max) NOT NULL,
    Username nvarchar(max) NOT NULL, Password nvarchar(max) NOT NULL,
    Discriminator nvarchar(13) NOT NULL);

IF OBJECT_ID(N'dbo.Categorias',N'U') IS NULL
CREATE TABLE dbo.Categorias(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY, Nombre nvarchar(max) NOT NULL);

IF OBJECT_ID(N'dbo.Capitulos',N'U') IS NULL
CREATE TABLE dbo.Capitulos(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Titulo nvarchar(max) NOT NULL, Texto nvarchar(max) NOT NULL,
    Discriminator nvarchar(21) NOT NULL,
    HistoriaId int NULL, VecesAlcanzado int NULL);

IF OBJECT_ID(N'dbo.Historias',N'U') IS NULL
CREATE TABLE dbo.Historias(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Titulo nvarchar(max) NOT NULL, Sinopsis nvarchar(max) NOT NULL,
    Estado int NOT NULL, CapituloInicialId int NULL,
    CONSTRAINT FK_Historias_Capitulos_CapituloInicialId FOREIGN KEY(CapituloInicialId) REFERENCES dbo.Capitulos(Id));

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Capitulos_Historias_HistoriaId')
ALTER TABLE dbo.Capitulos ADD CONSTRAINT FK_Capitulos_Historias_HistoriaId
    FOREIGN KEY(HistoriaId) REFERENCES dbo.Historias(Id);

IF OBJECT_ID(N'dbo.CategoriaHistoria',N'U') IS NULL
CREATE TABLE dbo.CategoriaHistoria(
    CategoriasId int NOT NULL, HistoriasId int NOT NULL,
    CONSTRAINT PK_CategoriaHistoria PRIMARY KEY(CategoriasId,HistoriasId),
    CONSTRAINT FK_CategoriaHistoria_Categorias_CategoriasId FOREIGN KEY(CategoriasId) REFERENCES dbo.Categorias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CategoriaHistoria_Historias_HistoriasId FOREIGN KEY(HistoriasId) REFERENCES dbo.Historias(Id) ON DELETE CASCADE);

IF OBJECT_ID(N'dbo.Opciones',N'U') IS NULL
CREATE TABLE dbo.Opciones(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY, Texto nvarchar(max) NOT NULL,
    CapituloDestinoId int NULL, CapituloIntermedioId int NULL,
    CONSTRAINT FK_Opciones_Capitulos_CapituloDestinoId FOREIGN KEY(CapituloDestinoId) REFERENCES dbo.Capitulos(Id),
    CONSTRAINT FK_Opciones_Capitulos_CapituloIntermedioId FOREIGN KEY(CapituloIntermedioId) REFERENCES dbo.Capitulos(Id));

IF OBJECT_ID(N'dbo.Lecturas',N'U') IS NULL
CREATE TABLE dbo.Lecturas(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId int NOT NULL, HistoriaId int NOT NULL,
    FechaInicio datetime2 NOT NULL, FechaFinalizacion datetime2 NULL,
    FinalAlcanzadoId int NULL, CapituloActualId int NULL,
    CONSTRAINT FK_Lecturas_Usuarios_UsuarioId FOREIGN KEY(UsuarioId) REFERENCES dbo.Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lecturas_Historias_HistoriaId FOREIGN KEY(HistoriaId) REFERENCES dbo.Historias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lecturas_Capitulos_FinalAlcanzadoId FOREIGN KEY(FinalAlcanzadoId) REFERENCES dbo.Capitulos(Id));

IF OBJECT_ID(N'dbo.Auditorias',N'U') IS NULL
CREATE TABLE dbo.Auditorias(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Accion nvarchar(max) NOT NULL, Fecha datetime2 NOT NULL,
    AdministradorId int NOT NULL, HistoriaId int NULL,
    CONSTRAINT FK_Auditorias_Usuarios_AdministradorId FOREIGN KEY(AdministradorId) REFERENCES dbo.Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Auditorias_Historias_HistoriaId FOREIGN KEY(HistoriaId) REFERENCES dbo.Historias(Id) ON DELETE SET NULL);

-- Los índices usados por las consultas y las relaciones.
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Capitulos_HistoriaId' AND object_id=OBJECT_ID('dbo.Capitulos'))
CREATE INDEX IX_Capitulos_HistoriaId ON dbo.Capitulos(HistoriaId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Historias_CapituloInicialId' AND object_id=OBJECT_ID('dbo.Historias'))
CREATE INDEX IX_Historias_CapituloInicialId ON dbo.Historias(CapituloInicialId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_CategoriaHistoria_HistoriasId' AND object_id=OBJECT_ID('dbo.CategoriaHistoria'))
CREATE INDEX IX_CategoriaHistoria_HistoriasId ON dbo.CategoriaHistoria(HistoriasId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Opciones_CapituloDestinoId' AND object_id=OBJECT_ID('dbo.Opciones'))
CREATE INDEX IX_Opciones_CapituloDestinoId ON dbo.Opciones(CapituloDestinoId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Opciones_CapituloIntermedioId' AND object_id=OBJECT_ID('dbo.Opciones'))
CREATE INDEX IX_Opciones_CapituloIntermedioId ON dbo.Opciones(CapituloIntermedioId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lecturas_UsuarioId' AND object_id=OBJECT_ID('dbo.Lecturas'))
CREATE INDEX IX_Lecturas_UsuarioId ON dbo.Lecturas(UsuarioId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lecturas_HistoriaId' AND object_id=OBJECT_ID('dbo.Lecturas'))
CREATE INDEX IX_Lecturas_HistoriaId ON dbo.Lecturas(HistoriaId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lecturas_FinalAlcanzadoId' AND object_id=OBJECT_ID('dbo.Lecturas'))
CREATE INDEX IX_Lecturas_FinalAlcanzadoId ON dbo.Lecturas(FinalAlcanzadoId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Auditorias_AdministradorId' AND object_id=OBJECT_ID('dbo.Auditorias'))
CREATE INDEX IX_Auditorias_AdministradorId ON dbo.Auditorias(AdministradorId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Auditorias_HistoriaId' AND object_id=OBJECT_ID('dbo.Auditorias'))
CREATE INDEX IX_Auditorias_HistoriaId ON dbo.Auditorias(HistoriaId);
