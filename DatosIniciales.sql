INSERT INTO dbo.Usuarios
    (NombreCompleto, Email, Username, Password, Discriminator)
VALUES
    ('Administrador Principal',
     'admin@obligatorio.com',
     'admin',
     'Admin123!',
     'Administrador');

INSERT INTO dbo.Categorias (Nombre)
VALUES
    ('Terror'),
    ('Fantasía'),
    ('Ciencia Ficción'),
    ('Aventura');