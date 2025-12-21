# Guia de Configuração e Migração (Unity)

Este guia foi personalizado com base na estrutura atual do seu projeto (Hierarchy) para facilitar a migração do sistema antigo para o novo.

---

## � Onde colocar cada componente (Baseado na sua Hierarchy)

Para que o sistema funcione com a estrutura que você já tem, siga exatamente estes passos:

### 1. Configurando a Lógica (Objeto `Inventory`)
No seu objeto chamado **`Inventory`** (que está entre `CraftingManager` e `Hud Controler`):
- **Adicione o componente:** `InventorySystem`
- **Adicione o componente:** `InventoryBridge` (necessário para a compatibilidade)
- *Configuração:* No `InventorySystem`, defina `Max Slots` (ex: 30).

### 2. Configurando a Interface (CONTROLE DO TAB)
⚠️ **Aviso Importante:** Se você coloca um script em um objeto e o desativa, o script "morre" e não consegue detecter o teclado. Por isso, não podemos colocar o `InventoryUI` no próprio objeto que ele vai desligar.

**Mude o componente de lugar:**
- **Remova** o script `InventoryUI` do objeto do Canvas.
- **Adicione** o script `InventoryUI` ao objeto **`Inventory`** (aquele da raiz que já tem o `InventorySystem`).
- Como esse objeto está sempre ativo, ele vai sempre "ouvir" o seu comando de **Tab**.

**Configure as referências no Inspector (dentro do objeto `Inventory`):**
- **Inventory:** Arraste o próprio objeto `Inventory`.
- **Inventory Panel:** Arraste o objeto **`InventoryUI`** que está lá no seu Canvas.
- **Slots Container:** Arraste o objeto **`ItemsGrid`** (que está dentro do `InventoryUI`).
- **Slot Prefab:** Arraste o seu prefab de slot.
- **Toggle Key:** `Tab`.

### 3. O Prefab de Slot
O seu objeto **`ItemsGrid`** deve estar vazio no início do jogo, pois o sistema criará os slots automaticamente.
- Crie um prefab de slot (com ícone e texto de quantidade).
- Adicione os componentes `UISlot` e `DragHandler` nele.
- Arraste esse prefab para o campo `Slot Prefab` do script que você colocou no objeto `Inventory`.

---

## 🔄 Como Migrar o que já existe

### No Objeto `Hud Controler`
Você tem um objeto chamado `Hud Controler`. Ele provavelmente usa o sistema antigo.
- **Não precisa deletar nada!** O `InventoryBridge` que colocamos no objeto `Inventory` traduzirá automaticamente os comandos do `Hud Controler` para o novo sistema.
- Certifique-se apenas que os itens que você criar tenham o campo **`Legacy Type`** preenchido (ex: o asset da madeira deve ter `Legacy Type: Wood`).

### No Objeto `CraftingManager`
O seu `CraftingManager` também continuará funcionando via ponte. 
- Quando ele pedir para o inventário remover um item, a ponte encontrará o item correto no novo sistema usando o enum `ItemType`.

---

## 🎨 Layout do `ItemsGrid`
Para que os slots fiquem alinhados como na sua estrutura:
1. Selecione o objeto **`ItemsGrid`**.
2. Adicione (se não tiver) um componente **`Grid Layout Group`**.
3. Configure o `Cell Size` (ex: 80x80) e o `Spacing`.
4. O `InventoryUI` vai instanciar os slots lá dentro e o Unity vai organizá-los automaticamente.
Antes de ver o resultado na tela, você precisa criar os itens:
1. Vá para `Assets/Resources/Items/`.
2. Crie um item (ex: `Wood`) via `Right-Click -> Create -> Inventory -> Items -> Resource Item`.
3. No campo **`Legacy Type`**, selecione `Wood`.
4. Isso é o que faz a "mágica" da ponte funcionar!

### Passo 2: Configurar o Prefab de Slot
O novo sistema cria os slots dinamicamente.
1. Crie um prefab para o slot (você pode usar um dos que já existem no seu `ItemsGrid` como base).
2. O prefab deve ter o componente **`UISlot`** e o **`DragHandler`**.
3. Arraste esse prefab para o campo **`Slot Prefab`** no componente `InventoryUI`.

---

## 🛠️ Como Migrar o Código (Exemplos Reais)

O seu projeto utiliza muito o `InventoryController`. Veja como adaptar:

### Exemplo: Coletáveis (Wood.cs, Fish.cs)
**Como está hoje:**
```csharp
FindObjectOfType<InventoryController>().Add(type, amount);
```

**Opção A (Ponte - Rápido):**
Não precisa mudar nada! O `InventoryBridge` vai interceptar as chamadas se você configurar o `Instance` corretamente.

**Opção B (Recomendado):**
```csharp
// Se quiser usar o novo sistema diretamente
InventorySystem.Instance.AddItem(meuItemData, 1);
```

### Exemplo: Crafting (CraftingManager.cs)
O Crafting é o ponto mais importante. Ele deve continuar verificando ingredientes normalmente via `InventoryBridge`.

---

## 💡 Recomendações para sua Estrutura

1. **Categorias:** No objeto **`Colectables Objects`** do seu HUD, você pode fazer com que ele mostre apenas itens da categoria `Resource`. No novo sistema, isso é fácil:
   ```csharp
   var resources = inventory.GetItemsByCategory(ItemCategory.Resource);
   ```

2. **Itens Iniciais:** No `InventorySystem`, você pode adicionar um recurso de "Starting Items" no futuro para o jogador já começar com algumas ferramentas.

3. **Nomes na Hierarchy:** Notei que seu objeto se chama `Hud Controler`. No código nós renomeamos para `HudController`. É uma boa prática renomear o objeto na Hierarchy também para manter a consistência.

---

## ❓ Perguntas Frequentes do seu Projeto

**"O que acontece com os itens que já tenho no dicionário antigo?"**
O novo sistema não usa mais o dicionário de enums. Ele usa uma lista de objetos. Ao abrir o jogo, o `InventoryBridge` servirá como tradutor.

**"Preciso mudar o enum ItemType?"**
Não! Nós o movemos para `Assets/Scripts/Core/ItemType.cs` justamente para que as receitas de Crafting e referências antigas não quebrem.

**"Como faço para o Drag and Drop funcionar no ItemsGrid?"**
Certifique-se que o objeto `ItemsGrid` tem um componente **`GridLayoutGroup`**. O `InventoryUI` vai organizar os slots automaticamente lá dentro.

---

## 📚 Links Úteis
- [Detailed Walkthrough](../../../.gemini/antigravity/brain/bc04e5e0-cd37-4d59-8730-c86cd9e0e16f/walkthrough.md)
- [Guia de Criação de Itens](creating-items-guide.md)
