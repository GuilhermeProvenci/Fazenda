# Guia: Como Criar um Novo Item

Este guia explica como adicionar novos itens ao seu jogo usando o novo sistema baseado em ScriptableObjects.

---

## 📁 Onde Salvar?

Todos os itens **DEVEM** ser salvos dentro da pasta:
`Assets/Resources/Items/`

Isso é necessário para que o sistema consiga carregar os itens dinamicamente e para que a ponte de compatibilidade (InventoryBridge) funcione.

---

## 🛠️ Criando o Item

1. Vá até a pasta `Assets/Resources/Items/`.
2. Clique com o botão direito → **Create** → **Inventory** → **Items**.
3. Escolha uma das categorias:
   - **Resource Item:** Madeira, Pedra, Peixe, etc.
   - **Consumable Item:** Comidas, Poções (itens que somem ao usar).
   - **Tool Item:** Machado, Picareta, Regador.
   - **Placeable Item:** Cerca, Baú, Construções.
   - **Seed Item:** Sementes para a fazenda.

---

## 📝 O que preencher? (Campos Importantes)

### 1. Basic Info
- **Item Name:** Nome que o jogador verá (ex: "Madeira de Carvalho").
- **Item ID:** Um nome único sem espaços (ex: "OakWood"). **Dica:** Use o mesmo nome do arquivo.
- **Description:** O texto que aparecerá quando o jogador passar o mouse.

### 2. Visual
- **Icon:** O Sprite que aparecerá no quadradinho do inventário.
- **World Prefab:** O modelo 3D ou Sprite 2D que aparecerá no chão quando você dropar o item.

### 3. Properties
- **Category:** Escolha a categoria correta para organização.
- **Is Stackable:** Marque se o jogador puder ter vários no mesmo slot (ex: 99 madeiras).
- **Max Stack Size:** Quantidade máxima por slot.

### 4. Legacy Compatibility (CRÍTICO) ⚠️
- **Legacy Type:** Este campo é o mais importante para a migração.
- Escolha o valor correspondente do seu `ItemType` antigo.
- **Exemplo:** Se estiver criando o item de Madeira, selecione `Wood` aqui. Isso faz com que scripts como o do `Player` ou `Crafting` continuem funcionando sem alterações.

---

## 🧪 Exemplo Prático: Criando a Cenoura

1. **Criar:** `Create -> Inventory -> Items -> Seed Item`.
2. **Nomear arquivo:** `Carrot`.
3. **Configurar:**
   - Name: "Cenoura"
   - Category: `Seed`
   - Is Stackable: `True`
   - Legacy Type: `Carrot`
   - **Growth Time:** (Específico de semente) Defina quanto tempo leva para crescer.
   - **Harvest Item:** Arraste aqui o item que o jogador ganha ao colher.

---

## 🚀 Recomendações

1. **Organização:** Crie subpastas dentro de `Items/` (ex: `Items/Resources`, `Items/Tools`) para não ficar bagunçado.
2. **IDs Únicos:** Nunca use o mesmo `Item ID` para itens diferentes.
3. **Sprites:** Tente manter os ícones com um tamanho padrão (ex: 64x64 ou 128x128) para que fiquem bonitos no inventário.
4. **Padrão de Mercado:** O sistema usa herança. Se você precisar de um item muito específico (ex: uma armadura que dá bônus de defesa), você pode criar um novo script que herda de `ItemData` e o Unity automaticamente mostrará a opção de criar esse novo tipo de item no menu!

---

## ✅ Checklist do Novo Item

- [ ] O arquivo está em `Resources/Items/`?
- [ ] O `Legacy Type` está selecionado?
- [ ] O ícone (Sprite) foi colocado?
- [ ] O `Item ID` é único?
- [ ] (Se ferramenta) O `Tool Type` está correto?
