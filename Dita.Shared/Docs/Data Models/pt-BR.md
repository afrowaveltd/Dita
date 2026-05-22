# Modelos de dados

O namespace define todas as estruturas de dados usadas em todo o sistema de localização e tradução — desde pares de requisição/resposta de API até relatórios de pipeline e instantâneos de painel.

## Vista geral do modelo

### Configuração

#### Configurações de Tradução Automática

Modelo de configuração de . Controla LibreTranslate server connection and pipeline behavior.

Propriedade
|---|---|---|---|
LibreTranslate servidor URL
Se é necessária uma chave API
Chave da API
Idioma padrão do aplicativo
Línguas a excluir da tradução
Pastas raiz da documentação
Activar a execução do gasoduto agendado
Atrasar antes da primeira execução
Minutos entre corridas
LibreTraduzir o ponto final do texto
Endpoint do arquivo LibreTranslate
Endpoint LibreTraduzir idiomas
endpoint de detecção da libretradução
Atraso entre pedidos de tradução
Tempo limite HTTP por solicitação
Se a configuração foi carregada

### LibreTranslate API models

#### TraduzirPedido → TraduzirResult

**Pedido** — chamada API de tradução de texto:

Propriedade
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

** Resultado** — resposta da tradução:

Propriedade
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Detecções

**Pedido**: **Resposta**:
**Response**: `{ Language, Confidence }`

#### TraduzirFileRequest → TraduzirFileResult

**Pedido**: **Resposta**:
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Entrada em língua única a partir do ponto final:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modelos de relatório de tubagens

#### VerificandoRelatório

Resultado da fase de validação do servidor:

Propriedade
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### TraduçãoRelatório

Resultado das fases de tradução do dicionário/país:

Propriedade
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### MarkdownTraduçõesRelatório

Resultado da etapa de tradução Markdown:

Propriedade
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Relatório de Armazenamento

Agregação final dos resultados persistentes:

Propriedade
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport<T>

Contêiner genérico que envolve qualquer tipo de relatório com metadados de fase:

Propriedade
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computado)

### Modelos de trabalho de tradução

#### FraseInFile

Item de trabalho para a fila de traduções:

Propriedade
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### Erro de Tradução

Registo de erro estruturado efectuado em todos os relatórios:

Propriedade
|---|---|
(código da língua, local do ficheiro ou nome do palco)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Tradução Única

Dicionário local único:

Propriedade
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Bloco extraído de um documento Markdown:

Propriedade
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modelos de resolução de texto

#### Localização do Texto Pedido → Localização de Texto Resposta

**Pedido** — localização baseada em dicionário (gravável):

Propriedade
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Resposta**:

Propriedade
|---|---|
(original)
(localizado)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationPedido → TextTranslationResponse

**Pedido** — tradução dinâmica (somente leitura):

Propriedade
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Resposta**:

Propriedade
|---|---|
(original)
(traduzido)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextoResoluçãoFonte

Identifica onde um valor localizado/traduzido foi resolvido a partir:

Valor
|---|---|
Encontrado no dicionário local para o idioma- alvo
Encontrado no dicionário de idiomas padrão
Não encontrado; adicionado ao dicionário padrão
Retornado por LibreTranslate
Retornado como está sem resolução

### Tipos partilhados

#### Definição do País

Entrada somente de leitura de :

Propriedade
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ComparaçãoCondição

Condições do filtro para avaliação:

Propriedade
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Resposta de Erro

Envelope de erro de API simples:

Propriedade
|---|---|
| `Error` | `string?` |
