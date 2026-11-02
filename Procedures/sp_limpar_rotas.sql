CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_rotas`()
BEGIN
-- Garante que o fuso horário esteja correto para o preenchimento da Auditoria

call crud_central.sp_limpar_rotas();

SET @now = UTC_TIMESTAMP();
SET @createdBy = 'Sistema';

-- Bloco de Carga da Tabela de Rotas baseada no OpenAPI
INSERT INTO rotas (Id, EndPoint, Rota, Metodo, Descricao, Menu, Created, CreatedBy)
VALUES
-- ----------------------------------------------------
-- Endpoints de Autenticação / Configuração Inicial
-- ----------------------------------------------------
(UUID(), 'SetupCadastroSqlite', '/auth/setup-cadastro-sqlite', 'POST', 'HTTP: POST /auth/setup-cadastro-sqlite', 'Configurações', @now, @createdBy),
(UUID(), 'SetupCadastro', '/auth/setup-cadastro', 'POST', 'HTTP: POST /auth/setup-cadastro', 'Configurações', @now, @createdBy),
(UUID(), 'Login', '/login', 'POST', 'HTTP: POST /login', 'Autenticação', @now, @createdBy),
(UUID(), 'EsqueciSenha', '/login/esqueci-senha', 'POST', 'HTTP: POST /login/esqueci-senha', 'Autenticação', @now, @createdBy),
(UUID(), 'ResetarSenha', '/login/resetar-senha', 'POST', 'HTTP: POST /login/resetar-senha', 'Autenticação', @now, @createdBy),

-- ----------------------------------------------------
-- Endpoints do Cadastro de Perfis
-- ----------------------------------------------------
(UUID(), 'GetPerfis', '/perfis', 'GET', 'HTTP: GET /perfis', 'Configurações', @now, @createdBy),
(UUID(), 'PostPerfis', '/perfis', 'POST', 'HTTP: POST /perfis', 'Configurações', @now, @createdBy),
(UUID(), 'GetPerfisById', '/perfis/{id}', 'GET', 'HTTP: GET /perfis/{id}', 'Configurações', @now, @createdBy),
(UUID(), 'PutPerfisById', '/perfis/{id}', 'PUT', 'HTTP: PUT /perfis/{id}', 'Configurações', @now, @createdBy),
(UUID(), 'DeletePerfisById', '/perfis/{id}', 'DELETE', 'HTTP: DELETE /perfis/{id}', 'Configurações', @now, @createdBy),
(UUID(), 'PatchPerfisById', '/perfis/{id}', 'PATCH', 'HTTP: PATCH /perfis/{id}', 'Configurações', @now, @createdBy),

-- ----------------------------------------------------
-- Endpoints do Cadastro de Usuários
-- ----------------------------------------------------
(UUID(), 'GetUsuarios', '/usuarios', 'GET', 'HTTP: GET /usuarios', 'Cadastros', @now, @createdBy),
(UUID(), 'PostUsuarios', '/usuarios', 'POST', 'HTTP: POST /usuarios', 'Cadastros', @now, @createdBy),
(UUID(), 'GetUsuariosById', '/usuarios/{id}', 'GET', 'HTTP: GET /usuarios/{id}', 'Cadastros', @now, @createdBy),
(UUID(), 'PutUsuariosById', '/usuarios/{id}', 'PUT', 'HTTP: PUT /usuarios/{id}', 'Cadastros', @now, @createdBy),
(UUID(), 'DeleteUsuariosById', '/usuarios/{id}', 'DELETE', 'HTTP: DELETE /usuarios/{id}', 'Cadastros', @now, @createdBy),
(UUID(), 'PatchUsuariosById', '/usuarios/{id}', 'PATCH', 'HTTP: PATCH /usuarios/{id}', 'Cadastros', @now, @createdBy);

INSERT INTO rotas (Id, EndPoint, Rota, Metodo, Descricao, Menu, Created, CreatedBy)
VALUES
(UUID(), 'GetDespesas', '/despesas', 'GET', 'HTTP: GET /despesas', 'Despesas', @now, @createdBy),
(UUID(), 'PostDespesas', '/despesas', 'POST', 'HTTP: POST /despesas', 'Despesas', @now, @createdBy),
(UUID(), 'GetDespesasById', '/despesas/{id}', 'GET', 'HTTP: GET /despesas/{id}', 'Despesas', @now, @createdBy),
(UUID(), 'PutDespesasById', '/despesas/{id}', 'PUT', 'HTTP: PUT /despesas/{id}', 'Despesas', @now, @createdBy),
(UUID(), 'PatchDespesasById', '/despesas/{id}', 'PATCH', 'HTTP: PATCH /despesas/{id}', 'Despesas', @now, @createdBy),
(UUID(), 'DeleteDespesasById', '/despesas/{id}', 'DELETE', 'HTTP: DELETE /despesas/{id}', 'Despesas', @now, @createdBy);

END;