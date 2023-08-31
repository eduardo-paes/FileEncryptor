# Aplicação CLI para Encriptação de Arquivos

Aplicação construída para encriptação de arquivos utilizando o método de encriptação AES.
Configurações utilizadas:

- Padding: `PKCS7`
- Mode: `CFB`
- BlockSize: `128`

## Funcionalidades Principais

- Encriptar todos arquivos de um diretório.
- Decriptar todos arquivos de um diretório.

## Arquivo de Configuração

A aplicação espera receber o nome do arquivo de configuração que será utilizado. O modelo de configuração do arquivo é mostrada abaixo:

```json
{
  "Source": "C://Path/To/Source/Directory",
  "Destination": "C://Path/To/Destination/Directory",
  "Key": "CHAVE_CRIPTOGRAFICA_TESTE",
  "Operation": "ENC", // "DEC",
  "EncSufix": ".bin",
  "DecSufix": ".txt"
}
```

O parâmetro `Operation` aceita o valor `ENC` para encriptação e `DEC` para decriptação.

Os parâmetros `EncSufix` e `DecSufix` definem respectivamente qual será o sufixo do arquivo encriptado e do arquivo decriptado.

O arquivo de configuração deve estar presente no caminho estipulado nas configurações da aplicação (`appsettings.json`), isto é, deve existir no diretório informado conforme caminho de exemplo abaixo:

```json
{
  "Settings": {
    "ConfigPath": "C://Path/To/Configs/Directory"
  }
}
```
