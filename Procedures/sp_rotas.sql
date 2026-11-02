CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_rotas`()
BEGIN
    -- Garante que o fuso horário esteja correto para o preenchimento da Auditoria
    CALL crud_central.sp_limpar_rotas();

    SET @now = UTC_TIMESTAMP();
    SET @createdBy = 'Sistema';

    -- Bloco de Carga da Tabela de Rotas baseada no OpenAPI completo
    INSERT INTO rotas (Id, EndPoint, Rota, Metodo, Descricao, Menu, Created, CreatedBy)
    VALUES
    -- ----------------------------------------------------
    -- Módulo: Autenticação / Configuração Inicial
    -- ----------------------------------------------------
    (UUID(), 'SetupCadastroSqlite', '/auth/setup-cadastro-sqlite', 'POST', 'HTTP: POST /auth/setup-cadastro-sqlite', 'Autenticação', @now, @createdBy),
    (UUID(), 'SetupCadastro', '/auth/setup-cadastro', 'POST', 'HTTP: POST /auth/setup-cadastro', 'Autenticação', @now, @createdBy),
    -- (UUID(), 'Login', '/login', 'POST', 'HTTP: POST /login', 'Autenticação', @now, @createdBy),
    -- (UUID(), 'EsqueciSenha', '/login/esqueci-senha', 'POST', 'HTTP: POST /login/esqueci-senha', 'Autenticação', @now, @createdBy),
    --  (UUID(), 'ResetarSenha', '/login/resetar-senha', 'POST', 'HTTP: POST /login/resetar-senha', 'Autenticação', @now, @createdBy),

    -- ----------------------------------------------------
    -- Módulo: Perfis de Usuário
    -- ----------------------------------------------------
    (UUID(), 'GetPerfis', '/perfis', 'GET', 'HTTP: GET /perfis', 'Configurações', @now, @createdBy),
    (UUID(), 'PostPerfis', '/perfis', 'POST', 'HTTP: POST /perfis', 'Configurações', @now, @createdBy),
    (UUID(), 'GetPerfisById', '/perfis/{id}', 'GET', 'HTTP: GET /perfis/{id}', 'Configurações', @now, @createdBy),
    (UUID(), 'PutPerfisById', '/perfis/{id}', 'PUT', 'HTTP: PUT /perfis/{id}', 'Configurações', @now, @createdBy),
    (UUID(), 'DeletePerfisById', '/perfis/{id}', 'DELETE', 'HTTP: DELETE /perfis/{id}', 'Configurações', @now, @createdBy),
    (UUID(), 'PatchPerfisById', '/perfis/{id}', 'PATCH', 'HTTP: PATCH /perfis/{id}', 'Configurações', @now, @createdBy),

    -- ----------------------------------------------------
    -- Módulo: Cadastro de Usuários
    -- ----------------------------------------------------
    (UUID(), 'GetUsuarios', '/usuarios', 'GET', 'HTTP: GET /usuarios', 'Cadastros', @now, @createdBy),
    (UUID(), 'PostUsuarios', '/usuarios', 'POST', 'HTTP: POST /usuarios', 'Cadastros', @now, @createdBy),
    (UUID(), 'GetUsuariosById', '/usuarios/{id}', 'GET', 'HTTP: GET /usuarios/{id}', 'Cadastros', @now, @createdBy),
    (UUID(), 'PutUsuariosById', '/usuarios/{id}', 'PUT', 'HTTP: PUT /usuarios/{id}', 'Cadastros', @now, @createdBy),
    (UUID(), 'DeleteUsuariosById', '/usuarios/{id}', 'DELETE', 'HTTP: DELETE /usuarios/{id}', 'Cadastros', @now, @createdBy),
    (UUID(), 'PatchUsuariosById', '/usuarios/{id}', 'PATCH', 'HTTP: PATCH /usuarios/{id}', 'Cadastros', @now, @createdBy),

    -- ----------------------------------------------------
    -- Módulo: Despesas
    -- ----------------------------------------------------
    (UUID(), 'GetDespesas', '/despesas', 'GET', 'HTTP: GET /despesas', 'Despesas', @now, @createdBy),
    (UUID(), 'PostDespesas', '/despesas', 'POST', 'HTTP: POST /despesas', 'Despesas', @now, @createdBy),
    (UUID(), 'GetDespesasById', '/despesas/{id}', 'GET', 'HTTP: GET /despesas/{id}', 'Despesas', @now, @createdBy),
    (UUID(), 'PutDespesasById', '/despesas/{id}', 'PUT', 'HTTP: PUT /despesas/{id}', 'Despesas', @now, @createdBy),
    (UUID(), 'PatchDespesasById', '/despesas/{id}', 'PATCH', 'HTTP: PATCH /despesas/{id}', 'Despesas', @now, @createdBy),
    (UUID(), 'DeleteDespesasById', '/despesas/{id}', 'DELETE', 'HTTP: DELETE /despesas/{id}', 'Despesas', @now, @createdBy),

    -- ----------------------------------------------------
    -- Módulo: Matriz de Níveis de Acesso (Novas Rotas)
    -- ----------------------------------------------------
    (UUID(), 'GetNiveisAcesso', '/niveis-acesso', 'GET', 'HTTP: GET /niveis-acesso', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'PostNiveisAcesso', '/niveis-acesso', 'POST', 'HTTP: POST /niveis-acesso', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'GetNiveisAcessoById', '/niveis-acesso/{id}', 'GET', 'HTTP: GET /niveis-acesso/{id}', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'PutNiveisAcessoById', '/niveis-acesso/{id}', 'PUT', 'HTTP: PUT /niveis-acesso/{id}', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'DeleteNiveisAcessoById', '/niveis-acesso/{id}', 'DELETE', 'HTTP: DELETE /niveis-acesso/{id}', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'GetNiveisAcessoRotas', '/niveis-acesso/rotas', 'GET', 'HTTP: GET /niveis-acesso/rotas', 'Níveis de Acesso', @now, @createdBy),
    (UUID(), 'PostNiveisAcessoMatriz', '/niveis-acesso/matriz', 'POST', 'HTTP: POST /niveis-acesso/matriz', 'Níveis de Acesso', @now, @createdBy);

END