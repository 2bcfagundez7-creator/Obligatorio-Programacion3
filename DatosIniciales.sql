-- DATOS DE PRUEBA - Escoge tu propia aventura (Programación 3)
-- Primero crear el esquema mediante migraciones de EF o EsquemaBD.sql.
-- Los IDs se obtienen exclusivamente mediante SCOPE_IDENTITY(), nunca se fijan a mano.
-- Este script se puede volver a ejecutar: no duplica el conjunto Demo P3.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.Historias WHERE Titulo LIKE N'P3 Demo 01 - %')
BEGIN
    PRINT N'Los datos P3 Demo ya existen; no se insertaron duplicados.';
    RETURN;
END;

BEGIN TRY
BEGIN TRANSACTION;

-- Usuarios de prueba (contraseñas solo de desarrollo).
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = 'admin')
    INSERT INTO dbo.Usuarios (NombreCompleto,Email,Username,Password,Discriminator)
    VALUES (N'Administrador principal',N'admin@p3.demo','admin','Admin123!','Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = 'demoadmin')
    INSERT INTO dbo.Usuarios (NombreCompleto,Email,Username,Password,Discriminator)
    VALUES (N'Administrador de prueba',N'demoadmin@p3.demo','demoadmin','Demo123!','Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = 'demoadmin2')
    INSERT INTO dbo.Usuarios (NombreCompleto,Email,Username,Password,Discriminator)
    VALUES (N'Segundo administrador de prueba',N'demoadmin2@p3.demo','demoadmin2','Demo123!','Administrador');

DECLARE @i int=1, @username varchar(40), @mail varchar(100);
WHILE @i <= 8
BEGIN
    SET @username = 'demolector' + RIGHT('00'+CONVERT(varchar(2), @i), 2);
    SET @mail = @username + '@p3.demo';
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = @username)
        INSERT INTO dbo.Usuarios (NombreCompleto,Email,Username,Password,Discriminator)
        VALUES (N'Lector de prueba ' + CONVERT(nvarchar(2),@i), @mail, @username,
                'Demo123!', 'Lector');
    SET @i += 1;
END;

-- 10 categorías para probar los filtros.
DECLARE @categorias table (Nombre nvarchar(100));
INSERT INTO @categorias VALUES
(N'Terror'),(N'Fantasía'),(N'Ciencia Ficción'),(N'Aventura'),(N'Misterio'),
(N'Drama'),(N'Humor'),(N'Suspenso'),(N'Ciencia'),(N'Historia');
INSERT INTO dbo.Categorias(Nombre)
SELECT c.Nombre FROM @categorias c
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categorias e WHERE e.Nombre = c.Nombre);

DECLARE @adminId int = (SELECT TOP 1 Id FROM dbo.Usuarios WHERE Username = 'demoadmin');
DECLARE @titulo nvarchar(200), @sinopsis nvarchar(max), @categoria nvarchar(100);
DECLARE @historiaId int, @inicialId int, @intermedioId int, @finalA int, @finalB int;
DECLARE @lectorId int, @fin int, @actual int, @j int, @cantidad int;
DECLARE @fecha datetime2;

SET @i=1;
WHILE @i <= 10
BEGIN
    -- Cada historia tiene dos finales distintos y opciones coherentes.

    IF @i = 1
    BEGIN
        SET @titulo = N'P3 Demo 01 - Ruta de las estrellas';
        SET @sinopsis = N'Una expedición espacial debe decidir entre salvar la nave o explorar una señal desconocida.';
        SET @categoria = N'Ciencia Ficción';
    END;
    IF @i = 2
    BEGIN
        SET @titulo = N'P3 Demo 02 - Casa de los espejos';
        SET @sinopsis = N'Una casona abandonada esconde una llave y dos salidas que nunca conducen al mismo lugar.';
        SET @categoria = N'Terror';
    END;
    IF @i = 3
    BEGIN
        SET @titulo = N'P3 Demo 03 - El faro silencioso';
        SET @sinopsis = N'La guardia del faro debe resolver la desaparición de un barco antes del amanecer.';
        SET @categoria = N'Misterio';
    END;
    IF @i = 4
    BEGIN
        SET @titulo = N'P3 Demo 04 - Bosque de cenizas';
        SET @sinopsis = N'Una caminante escucha un llamado que cambiará el destino de su aldea.';
        SET @categoria = N'Fantasía';
    END;
    IF @i = 5
    BEGIN
        SET @titulo = N'P3 Demo 05 - La ciudad sumergida';
        SET @sinopsis = N'Una arqueóloga descubre una antigua ciudad debajo del río.';
        SET @categoria = N'Aventura';
    END;
    IF @i = 6
    BEGIN
        SET @titulo = N'P3 Demo 06 - El último tren';
        SET @sinopsis = N'Los pasajeros de un tren nocturno deben elegir a quién creer.';
        SET @categoria = N'Suspenso';
    END;
    IF @i = 7
    BEGIN
        SET @titulo = N'P3 Demo 07 - Código Aurora';
        SET @sinopsis = N'Un equipo intenta detener una inteligencia artificial fuera de control.';
        SET @categoria = N'Ciencia';
    END;
    IF @i = 8
    BEGIN
        SET @titulo = N'P3 Demo 08 - La carta perdida';
        SET @sinopsis = N'Dos amistades se reencuentran a través de una carta que nadie esperaba recibir.';
        SET @categoria = N'Drama';
    END;
    IF @i = 9
    BEGIN
        SET @titulo = N'P3 Demo 09 - El torneo absurdo';
        SET @sinopsis = N'Un torneo imposible exige decisiones ingeniosas y algo de suerte.';
        SET @categoria = N'Humor';
    END;
    IF @i = 10
    BEGIN
        SET @titulo = N'P3 Demo 10 - El portal del tiempo';
        SET @sinopsis = N'Una investigadora visita un período olvidado y altera el curso de la historia.';
        SET @categoria = N'Historia';
    END;

    INSERT INTO dbo.Historias(Titulo,Sinopsis,Estado,CapituloInicialId)
    VALUES (@titulo,@sinopsis,0,NULL);
    SET @historiaId = CONVERT(int,SCOPE_IDENTITY());

    INSERT INTO dbo.Capitulos(Titulo,Texto,Discriminator,HistoriaId,VecesAlcanzado)
    VALUES (N'El comienzo', N'Todo empieza con una decisión. ¿Seguís investigando o volvés a casa?',
            'CapituloIntermedio',@historiaId,NULL);
    SET @inicialId = CONVERT(int,SCOPE_IDENTITY());

    INSERT INTO dbo.Capitulos(Titulo,Texto,Discriminator,HistoriaId,VecesAlcanzado)
    VALUES (N'Una nueva pista', N'El viaje continúa y aparece otra posibilidad.',
            'CapituloIntermedio',@historiaId,NULL);
    SET @intermedioId = CONVERT(int,SCOPE_IDENTITY());

    INSERT INTO dbo.Capitulos(Titulo,Texto,Discriminator,HistoriaId,VecesAlcanzado)
    VALUES (N'Final: volver a casa', N'El personaje regresa a casa y lleva consigo una lección.',
            'CapituloFinal',@historiaId,0);
    SET @finalA = CONVERT(int,SCOPE_IDENTITY());

    INSERT INTO dbo.Capitulos(Titulo,Texto,Discriminator,HistoriaId,VecesAlcanzado)
    VALUES (N'Final: resolver el misterio', N'La protagonista supera el desafío y descubre toda la verdad.',
            'CapituloFinal',@historiaId,0);
    SET @finalB = CONVERT(int,SCOPE_IDENTITY());

    INSERT INTO dbo.Opciones(Texto,CapituloDestinoId,CapituloIntermedioId)
    VALUES (N'Continuar investigando',@intermedioId,@inicialId),
           (N'Volver a casa',@finalA,@inicialId),
           (N'Seguir la nueva pista hasta el final',@finalB,@intermedioId);

    UPDATE dbo.Historias SET CapituloInicialId=@inicialId,Estado=1 WHERE Id=@historiaId;
    INSERT INTO dbo.CategoriaHistoria(CategoriasId,HistoriasId)
    SELECT TOP 1 Id,@historiaId FROM dbo.Categorias WHERE Nombre=@categoria ORDER BY Id;
    -- Agregamos una segunda categoría por historia.
    INSERT INTO dbo.CategoriaHistoria(CategoriasId,HistoriasId)
    SELECT TOP 1 Id,@historiaId FROM dbo.Categorias
    WHERE Nombre<>@categoria AND Nombre IN (N'Aventura',N'Misterio',N'Fantasía')
    ORDER BY Id;

    INSERT INTO dbo.Auditorias(Accion,Fecha,AdministradorId,HistoriaId)
    VALUES (N'Crear historia',DATEADD(day,-30,GETDATE()),@adminId,@historiaId),
           (N'Publicar historia',DATEADD(day,-29,GETDATE()),@adminId,@historiaId);

    -- Frecuencias diferentes para poder comparar RF09, RF10 y RF11.
    SET @cantidad = CASE WHEN @i=1 THEN 7 WHEN @i=2 THEN 6
                         WHEN @i=3 THEN 5 WHEN @i=4 THEN 4 ELSE 2 END;
    SET @j=1;
    WHILE @j<=@cantidad
    BEGIN
        SET @username = 'demolector'+RIGHT('00'+CONVERT(varchar(2),((@i+@j-2)%8)+1),2);
        SET @lectorId = (SELECT Id FROM dbo.Usuarios WHERE Username=@username);
        SET @fin = CASE WHEN @j%3=0 THEN @finalA ELSE @finalB END;
        SET @fecha=DATEADD(day,-(@i*2+@j),GETDATE());
        INSERT INTO dbo.Lecturas(UsuarioId,HistoriaId,FechaInicio,FechaFinalizacion,FinalAlcanzadoId,CapituloActualId)
        VALUES (@lectorId,@historiaId,@fecha,DATEADD(minute,15,@fecha),@fin,@fin);
        SET @j += 1;
    END;
    -- Cada dos historias dejamos una lectura sin terminar.
    IF @i%2=0
    BEGIN
        SET @lectorId=(SELECT TOP 1 Id FROM dbo.Usuarios WHERE Username='demolector01');
        INSERT INTO dbo.Lecturas(UsuarioId,HistoriaId,FechaInicio,FechaFinalizacion,FinalAlcanzadoId,CapituloActualId)
        VALUES (@lectorId,@historiaId,GETDATE(),NULL,NULL,@inicialId);
    END;
    SET @i += 1;
END;

-- Contador de cada final consistente con las lecturas efectivamente completadas.
UPDATE c SET VecesAlcanzado=(SELECT COUNT(*) FROM dbo.Lecturas l
                             WHERE l.FinalAlcanzadoId=c.Id AND l.FechaFinalizacion IS NOT NULL)
FROM dbo.Capitulos c WHERE c.Discriminator='CapituloFinal';

COMMIT TRANSACTION;
PRINT N'Datos P3 Demo cargados: 10 historias, 40 capítulos, 30 opciones y lecturas de prueba.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
