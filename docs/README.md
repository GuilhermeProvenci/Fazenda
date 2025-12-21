# Documentação do Sistema de Inventário

Bem-vindo à documentação do novo sistema de inventário dinâmico!

---

## 📚 Guias Disponíveis

### 🔨 [Criando Novos Itens](creating-items-guide.md)

Aprenda a criar itens ScriptableObjects:
- Escolher o tipo correto de item
- Configurar propriedades básicas e específicas
- Organizar arquivos
- Testar itens
- Boas práticas

**Quando usar:** Ao adicionar qualquer item novo ao jogo.

---

### 🎮 [Configuração no Unity](unity-setup-guide.md)

Configure o sistema no Unity Editor:
- Setup do InventorySystem
- Criar UI do inventário
- Configurar drag & drop
- Mapear itens para compatibilidade
- Migrar do sistema antigo
- Testes e debug

**Quando usar:** Ao configurar o sistema pela primeira vez ou migrar do sistema antigo.

---

### 📖 [Walkthrough Completo](../../../.gemini/antigravity/brain/bc04e5e0-cd37-4d59-8730-c86cd9e0e16f/walkthrough.md)

Documentação técnica completa:
- Arquitetura do sistema
- Todas as classes e métodos
- Estrutura de arquivos
- API completa
- Vantagens do novo sistema

**Quando usar:** Para entender o sistema em profundidade ou fazer modificações avançadas.

---

### 📋 [Plano de Implementação](../../../.gemini/antigravity/brain/bc04e5e0-cd37-4d59-8730-c86cd9e0e16f/implementation_plan.md)

Detalhes da implementação:
- Análise do sistema antigo
- Nova arquitetura proposta
- Código de exemplo
- Plano de migração

**Quando usar:** Para entender decisões de design ou planejar extensões.

---

## 🚀 Quick Start

### Para Iniciantes

1. **Leia:** [Configuração no Unity](unity-setup-guide.md)
2. **Configure:** Sistema básico no Unity
3. **Crie:** Primeiro item seguindo [Criando Itens](creating-items-guide.md)
4. **Teste:** Abra inventário e teste drag & drop

### Para Desenvolvedores

1. **Leia:** [Walkthrough Completo](../../../.gemini/antigravity/brain/bc04e5e0-cd37-4d59-8730-c86cd9e0e16f/walkthrough.md)
2. **Entenda:** Arquitetura e classes
3. **Migre:** Código antigo usando InventoryBridge
4. **Estenda:** Adicione funcionalidades customizadas

---

## 📁 Estrutura do Projeto

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── ItemCategory.cs
│   │   └── ItemType.cs (legado)
│   │
│   ├── Inventory/
│   │   ├── Data/              (Classes de itens)
│   │   ├── Runtime/           (Sistema de inventário)
│   │   └── UI/                (Interface)
│   │
│   └── [Outros scripts...]
│
├── Resources/
│   └── Items/                 (ScriptableObjects dos itens)
│       ├── Resources/
│       ├── Consumables/
│       ├── Tools/
│       ├── Placeables/
│       └── Seeds/
│
├── Prefabs/
│   └── UI/
│       └── InventorySlot.prefab
│
└── docs/                      (Esta documentação)
    ├── README.md
    ├── creating-items-guide.md
    └── unity-setup-guide.md
```

---

## 🎯 Conceitos Principais

### ItemData (ScriptableObject)

Representa um item do jogo. Cada item é um arquivo `.asset` que pode ser configurado no Inspector.

**Tipos disponíveis:**
- `ResourceItem` - Recursos naturais
- `ConsumableItem` - Itens consumíveis
- `ToolItem` - Ferramentas
- `PlaceableItem` - Objetos colocáveis
- `SeedItem` - Sementes

### InventorySystem

Gerencia todos os slots e itens do inventário. Singleton acessível via `InventorySystem.Instance`.

**Principais métodos:**
- `AddItem(ItemData, int)` - Adiciona item
- `RemoveItem(ItemData, int)` - Remove item
- `MoveItem(fromIndex, toIndex)` - Move item
- `HasItem(ItemData, int)` - Verifica se tem item

### InventoryBridge

Camada de compatibilidade que permite código antigo (usando `ItemType` enum) funcionar com o novo sistema.

**Uso:**
```csharp
InventoryBridge.Instance.Add(ItemType.Wood, 5);
```

### InventoryUI

Controla a interface visual do inventário, criando slots dinamicamente e gerenciando drag & drop.

---

## 💡 Exemplos de Código

### Adicionar Item

```csharp
// Novo sistema
var wood = Resources.Load<ItemData>("Items/Wood");
InventorySystem.Instance.AddItem(wood, 5);

// Sistema legado (via bridge)
InventoryBridge.Instance.Add(ItemType.Wood, 5);
```

### Verificar Item

```csharp
// Novo sistema
var wood = Resources.Load<ItemData>("Items/Wood");
if (InventorySystem.Instance.HasItem(wood, 3))
{
    Debug.Log("Tem madeira suficiente!");
}

// Sistema legado
if (InventoryBridge.Instance.Has(ItemType.Wood, 3))
{
    Debug.Log("Tem madeira suficiente!");
}
```

### Usar Item

```csharp
var potion = Resources.Load<ConsumableItem>("Items/HealthPotion");
bool consumed = potion.Use(playerGameObject);
if (consumed)
{
    InventorySystem.Instance.RemoveItem(potion, 1);
}
```

### Eventos

```csharp
void Start()
{
    InventorySystem.Instance.OnItemAdded += OnItemAdded;
    InventorySystem.Instance.OnInventoryChanged += UpdateUI;
}

void OnItemAdded(InventorySlotData slot)
{
    Debug.Log($"Added {slot.Item.itemName} x{slot.Quantity}");
}
```

---

## 🔧 Customização

### Criar Novo Tipo de Item

1. Crie nova classe herdando de `ItemData`:

```csharp
[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Items/Weapon Item")]
public class WeaponItem : ItemData
{
    [Header("Weapon Properties")]
    public int damage;
    public float attackSpeed;
    
    public override bool Use(GameObject user)
    {
        // Lógica de equipar arma
        return false; // Não consome
    }
}
```

2. Crie itens desse tipo no Unity
3. Use normalmente no InventorySystem

### Adicionar Categoria

1. Edite `ItemCategory.cs`:

```csharp
public enum ItemCategory
{
    // ... existentes
    Weapon,  // Nova categoria
}
```

2. Configure categoria nos itens

---

## ❓ FAQ

### Posso usar os dois sistemas ao mesmo tempo?

Sim! O `InventoryBridge` permite que ambos coexistam. Perfeito para migração gradual.

### Preciso recriar todos os itens?

Não imediatamente. Use o Bridge para manter compatibilidade enquanto cria os ScriptableObjects gradualmente.

### Como salvar o inventário?

Salve o `itemID` e `quantity` de cada slot. Ao carregar, use `Resources.Load<ItemData>()` com o ID.

### Posso ter mais de 30 slots?

Sim! Configure `Max Slots` no InventorySystem ou ative `Auto Expand`.

### Como adicionar tooltips?

Implemente um sistema de tooltip que chama `item.GetTooltip()` ao passar o mouse sobre o slot.

---

## 🐛 Suporte

### Problemas Comuns

Consulte a seção "Problemas Comuns" em:
- [Unity Setup Guide](unity-setup-guide.md#-problemas-comuns)
- [Creating Items Guide](creating-items-guide.md#-problemas-comuns)

### Debug

Use os context menus disponíveis:
- InventorySystem → `Debug: Print Inventory`
- InventoryBridge → `Print Mappings`
- InventoryUI → `Force Refresh`

---

## 🎯 Roadmap

### Implementado ✅

- Sistema de itens OOP
- Inventário dinâmico
- Drag & drop
- UI completa
- Compatibilidade com código antigo

### Próximas Funcionalidades 🚧

- Sistema de tooltips visual
- Filtros por categoria
- Busca de itens
- Hotbar (quick slots)
- Sistema de peso/carga
- Sorting automático
- Item splitting (dividir pilhas)

---

## 📞 Contato

Para dúvidas ou sugestões sobre o sistema, consulte a documentação ou verifique os comentários no código.

---

**Última atualização:** 20/12/2025
**Versão do Sistema:** 1.0
