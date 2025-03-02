using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.MindMapViewer;

public class NodePropertiesPanel : Panel {
    
	// TODO: proper parsing of entries from Templates when possible.
	private static readonly Dictionary<string, string> EngineIdToImageMap = new() {
		["b6c7b430e1da09f4484d00ded9944f91"] = "time_01",
		["d83e9c702be38fb4492ddeea839bf070"] = "rat_02",
		["f8d438b03bdf54446b7021c2d05f3c25"] = "sobor_01",
		["34ff7631263ccf2409c014b072ab1ed0"] = "birdmask_03",
		["166f51224fb3c144e9ed80804b0e2b52"] = "grave_01",
		["15a8b8032d267574e86c105249dec077"] = "window_01",
		["9867a215bf645874aac676a8de1c093b"] = "bottle_01",
		["c53d9745f77728640a02eeff36beb54f"] = "dice_01",
		["60278b8513adeae4aac53b541f5f53d5"] = "quarantine_01",
		["b37179d52223e52409f88b4894a5ffcb"] = "birdmask_02",
		["c9839a78b46ea6743931fb780e6a38ed"] = "ospina_01",
		["490bcb398af3c4e4ab21968022931c10"] = "wheel_01",
		["24d51fc9e501bf7479babc0209b1454a"] = "kids_01",
		["b011c82a248d030449ef02d18ca828ff"] = "heart_02",
		["abf0ca7a5c77edf4b8326565fb0bd183"] = "birdmask_01",
		["abee569a054f4f24cbbfa4a42f994591"] = "medicine_01",
		["1be3f0baf487c4b4da71f04bf9d496ba"] = "root_01",
		["10bdbfba0c35fd84fb66b5746e5f63eb"] = "heart_04",
		["17be22db3c2a7ce4fa667adccf97c0f7"] = "camp_01",
		["5f7d764c32548eb47a6eea0b9cc0f651"] = "ospina_02",
		["7543f7bcd35828847a7c82a955adc328"] = "grave_02",
		["901127dcd9781ee4bb2ac08f86b306cc"] = "rat_01",
		["4aea639eefe41bb488841f63f3b11eaf"] = "heart_03",
		["6ef847aeaeeafd04eb2d3eb0ffe6fee0"] = "grave_03",
		["b394efaed1d125d44966cb725b3f0f43"] = "heart_01",
		["b8c2578fcf98c96488f00c0ce64e271f"] = "clerks_01",
		["52e255afef6ba3c4ba4e734377a41524"] = "key_01",
		["3bd5babf67067b44aae092441b1630af"] = "justice_01",
		["c45b9089a1e54abaa43b09e61425049d"] = "abbatoir",
		["80789a68be0c4ede878a3531fa57ceb8"] = "bacteria",
		["3d7f0755fd5a432aac6661c99fe7e242"] = "barricade",
		["d1d092632ba2438d8e059688f1c09dd0"] = "blood",
		["3f1c66a299a944d59f02c069c930ac83"] = "bull_01",
		["df9c7a3321f54425b8697d1e46b22790"] = "bull_02",
		["d926161671d1496781de55cff9f8a5ba"] = "bull_03",
		["65e41318428a4d01888b1e0f899ee3c7"] = "chains",
		["a6d65c2eff7b4b2b96918eb10b6bd969"] = "crack_01",
		["3b96323a75cd4587b072978968f7a851"] = "crack_02",
		["33f1bcb17dd44a3aa0d3cc92e656a6e7"] = "crack_03",
		["8fcafdee34ad4bed898a220b6cdf6109"] = "dvoedushnik",
		["b81bbf357a794801abe3a217c531b632"] = "eye",
		["8fec130756d14439b1c77af36389afe4"] = "fingerprint",
		["5b850800d9fa4f8bb492326cb47ea6b8"] = "fire",
		["f1ae28b2c312480990eb4ec6a71eb2b5"] = "fist",
		["8bd97656af7f482085bf6a8db5b52c4d"] = "footprint",
		["09bf5004d6174b09a1d8c0e6b6b7f205"] = "gesture_01",
		["def611897b1244d29699f5d90096596d"] = "gesture_02",
		["e12d3e81b5bb4b6c979919a5d7fe1b07"] = "gesture_03",
		["fdcedc7afd9745978bf445bb5e7cd4e5"] = "grave",
		["cd31de9b48a340fd89ba642220180271"] = "gun",
		["c34c62fab9614432ada2ef5117064de2"] = "handshake_01",
		["fccafc2d3fc44615b48900c0655e80f2"] = "handshake_02",
		["081bcf2d4a1244c9831e582b4065dbb5"] = "handshake_03",
		["4a8be5e7469a49d091eaa2d41ec282ba"] = "iron_heart",
		["264a96905f9f4f0eb1a3d82f0918cdbe"] = "knife",
		["0aebf5b4cda04a39bc905c141f5aaf30"] = "knot",
		["4844891517094ec3bc80a80acde6bd9d"] = "lips",
		["6fab2b7c9b204dc18acac201b247b162"] = "lock",
		["43d5e935c5d6480ba9657bf4baf9c465"] = "needle",
		["ad70d4f0ec604057b94e4779421883c7"] = "pills",
		["2ad2fe22d08a4e96bf72355f04f8801a"] = "polyhedron",
		["96c98e23b0db475d8c367821bd5d1eb3"] = "psiglavez",
		["809c42dbf931497da34fd5e9243bb719"] = "sad_woman",
		["741391b041a84a049b4df8db88412efd"] = "sad_woman2",
		["7ae51258d78a43a2aafa01a947300273"] = "sad_woman3",
		["eb2606283c6c40f5802703f1511b6660"] = "scalpel",
		["45e9b2f6089b4dee95e292002ef9b5bf"] = "shabnak",
		["8ac7c91a43fc4beca3a9409f09cb5a8a"] = "sky",
		["8a78ce1457a74b3aae052c1bd639a548"] = "stairway",
		["f4fde150a0834e7f83e62452d35b61c9"] = "stairway2",
		["08c8bd734dde40b9bddfbfaf18708dda"] = "tavro_01",
		["d8d2e991d92c4cebbaa2e97410331a3c"] = "tavro_02",
		["5849d8c5b50d41e29b71120e092da82d"] = "tavro_03",
		["592d327db10c449db4185850f0da7581"] = "theatre",
		["3d546a9b2d7945568cca72f4fdf38783"] = "tragic",
		["e79d7bc71e1d4bf68c68387c5debe670"] = "water",
		["6e40b1ae348c41fcae4c7383e8d985e7"] = "withachild",
		["e5c1300ca9dbf16428d56c3baab2e3f0"] = "aglaya",
		["213ea13d1d92c9c409278c42ab95e9f4"] = "ammo",
		["f4031b42e0c17404b8f81d74f1f90805"] = "beer",
		["dcdcf74351d211d4ea74737f0b27172d"] = "bell",
		["21c56e9686cb5824aad2418824a19e65"] = "bigdevice",
		["901bde04aba33d542bf3e97db32e3f18"] = "bottleempty",
		["803ba3112fad13c44b45f19211f475eb"] = "bottlefull",
		["4fa4e6d35df791249acd22a593d6d098"] = "brain",
		["a620a436edae80647bd0fbd36efd64a7"] = "braintwo",
		["9d43eeecc94633746990754e9ce8846e"] = "bread",
		["8a4d29ab9021c3744b90f18438b9faf1"] = "brickwall",
		["fb912ffc6c0805841bc13b5c1668bcbd"] = "cap",
		["27d20535e25d6184c95a734a9128fc38"] = "cards",
		["35163ef9ae194554a887a4f318fd6294"] = "clock",
		["749f37e7a7ccc044e893c0400103bf13"] = "coins",
		["5783be5a15514a144be61a4c92557d40"] = "coinshand",
		["ded530ee1ba970e4785c39f5e863ff4b"] = "dagger",
		["df4b7a5f40663b44d84f2ffeba7709a4"] = "doorcross",
		["6a70c84f8215add4ab8f0d8180a96d4a"] = "eyelock",
		["0ff74230336d4fb40bd44ec50e52b594"] = "factory",
		["0b70de1f9f4764d4a93e275f346009c3"] = "gasmask",
		["8f3509627040a774f9f050fe52663e3f"] = "gears",
		["8d03481e329867449afb0879589da6f8"] = "grif",
		["4500ad05e3cb16540bdd3a36859c5694"] = "hammer",
		["676f8d80b9459d2438c429a5acd41083"] = "herbs",
		["923b2fb0701ebc645b08eb7a6e3cab58"] = "kapella",
		["aed536ed9595f7f46ba9cbf404c76917"] = "keylock",
		["5f31bdccd3ae24d49a37d779882527d0"] = "keystring",
		["4491262a4752e04409b45a7c19a3b886"] = "khan",
		["e661909e61416974b85bd13578e4b1fb"] = "kidney",
		["407a1f5e1ed7d244c83192479cf3f3e5"] = "kneel",
		["c3e3e825b097a6e428a791485a1c3aac"] = "lara",
		["c5b467321be73b54a8006192d3a0c484"] = "laska",
		["159fda79149ad9c4cba7ddc7473bd2bf"] = "letter",
		["c7cc79f35d4cbaf46a74cc2218cca395"] = "liver",
		["b80bc4561e908a049a0b160f1412f55a"] = "locomotive",
		["20de79a42fb4abd41bf6ed268254c9d7"] = "man",
		["1115aef5c2050ba43875e37927a14c9b"] = "mishka",
		["21725cbbf0695c24ca4fdfbbc2a1923b"] = "molotov",
		["02061219c5674e143bd447d7fb46b400"] = "moon",
		["096da8708780a634d8ed9029b3de74d5"] = "mortar",
		["e3d7710b2f4ae47469b56b9d2b525f1a"] = "notkin",
		["f849afcf9b445564b9027ea67bc76506"] = "odong 1",
		["34c31fe7c4557a641a99dc5061052b33"] = "odong",
		["5d072f3931d15274b96d3809ff310a49"] = "olgimsky",
		["33055aebc0fab8642a467d4c4f0ebafb"] = "paperone",
		["2d34ad6ae98a2c64ebdf33ec1dae736d"] = "papertwo",
		["606fda3e8b0a1714e86d94275b1e6eaf"] = "purse",
		["7b59f7dd4af91394f9bac9d001c1601b"] = "revolver",
		["b4bf85533a2141c499734cec6f256a5e"] = "rifle",
		["13ecf6b6c0474f3428bd9b553c1b5241"] = "rubin",
		["d1348c807e1dc614cb6399fc3a19e171"] = "saburov",
		["f80ed81caadf4e87b721e8ba2bdf7208"] = "skullbones",
		["9596c86eb5e14c0f811245897835b4bc"] = "snakes",
		["9bb755a8ca69482cb78577d5feb1b890"] = "soldier",
		["b8f8d996b0cee68469fc9178ec124f78"] = "spichka",
		["35e0c0c8febb14349857c804d290baa3"] = "taya",
		["8f887565713941dfa8f15137c5aa8548"] = "teeth",
		["117c9e6996ed4dc09010ef63ba30cc64"] = "tincan",
		["41066ce472064e0b86aa136f9fd34ff4"] = "torch",
		["c30e8277610b400bb54d29e12812cb08"] = "train",
		["4f00855d95c048b482bf8cac26489837"] = "tunnel",
		["23da4b8dfd994d33af3a079b77228395"] = "vial",
		["a18f1e60c6a540749f937a1b879e45ae"] = "woman",
		["327177c73d785f54bbe45d30858238af"] = "bride",
		["b1f13704bab0718428519a1f139c5e9b"] = "house_01",
		["1c8434683fac44a9a0f589d457b55e9b"] = "progress_3_0",
		["a5d8ceb8962e4f62ac9e524f6e25436a"] = "progress_3_bottleful_1",
		["d7f556875512468391a0c0ffcaa107c2"] = "progress_3_bottleful_2",
		["961570553c3740d186fe3023938431a3"] = "progress_3_bottleful_3",
		["b814cb7787d041a2aa4c9b109cdd6f2e"] = "progress_4_0",
		["06feb54bffa64f1ebaf8991ba9989a7a"] = "progress_4_medicine_1",
		["1125bfa68d024d17bac929f308660b8f"] = "progress_4_medicine_2",
		["4e19ca90e3b64ec1a9b51b0d803849d7"] = "progress_4_medicine_3",
		["8810698d782f418c8353451d0e389f68"] = "progress_4_medicine_4",
		["4de3acea6ab84bbba2c3a381f022c345"] = "progress_6_0",
		["cdb49f86db8a4b7abef31ee7cb12a762"] = "progress_6_scalpel_1",
		["9b3c125c56764643b4fb9fe80eb1b217"] = "progress_6_scalpel_2",
		["f65b12862684479b98b7f63cb129e613"] = "progress_6_scalpel_3",
		["8732777aa1d4461b80a97570d22877f1"] = "progress_6_scalpel_4",
		["5ecc892460004cb6bcddb0e850a81391"] = "progress_6_scalpel_5",
		["539305c8428a41388bb77257c2108d82"] = "progress_6_scalpel_6",
		["a6cfc98079e8489b993ab70e03e41db1"] = "progress_4_fingerprint_1",
		["422a84db300f43089523b4aa28a32300"] = "progress_4_fingerprint_2",
		["2729a3d17f82455eb6c882b9889848bf"] = "progress_4_fingerprint_3",
		["274a035c29694221bdd0cc6e7f6f25dc"] = "progress_4_fingerprint_4",
		["ddba2916534b4bebbc4a9828e4eb42db"] = "progress_6_candles_1",
		["deca964821294e9f8777d735f9d19b0c"] = "progress_6_candles_2",
		["49e7c50a9e434b6ea5bd081f0a09a5a6"] = "progress_6_candles_3",
		["475efbac654d42de8bdeb10f3597a662"] = "progress_6_candles_4",
		["77dc1b3a0639439aa097f285557b3a1a"] = "progress_6_candles_5",
		["4a961bb7aee544fdafb20b083daf0e6a"] = "progress_6_candles_6",
		["1f9336be0944422e910320d20db309af"] = "progress_4_herbs_1",
		["45f919d0110742fcbfc33761384dd933"] = "progress_4_herbs_2",
		["2d795537eae8489eb41dcc02f12b5889"] = "progress_4_herbs_3",
		["25ab893bd48f4fc3a5f6c145b8df3f66"] = "progress_4_herbs_4",
		["62af7f27cd0c4f23828bd95006746735"] = "progress_4_odong_1",
		["168ceb2287df4e168574c55a52db662f"] = "progress_4_odong_2",
		["924d050b79654737b8e3044373415a61"] = "progress_4_odong_3",
		["0d8dfbc4a28c462eadb6fdba5a7454b5"] = "progress_4_odong_4",
		["5ba8a60c49b041b08dbca0a41a6827f9"] = "progress_4_purse_1",
		["d8fa66583fc34a4b886c4351dff3b123"] = "progress_4_purse_2",
		["258a43859e1645029f92cf619c3e121a"] = "progress_4_purse_3",
		["6ef51add81574b3587a51d59933a67e9"] = "progress_4_purse_4",
		["c370cc7ee625400daeea1645852e9a0d"] = "progress_3_clock_1",
		["99ca903269594ecd8481babbdf64feae"] = "progress_3_clock_2",
		["7aa43590168c42968dc246c9f397c631"] = "progress_3_clock_3",
		["3ba795382a0949048ad7e5ac78d96994"] = "braintwo_inverted",
		["df81681a6eee5994299fa327f4a31e94"] = "brain_inverted",
		["6a9738dc85540314fb1ef1ed182535b3"] = "kidney_inverted",
		["d9a3a1ed453f9644783da7541c6c6ed6"] = "liver_inverted",
		["534013320b273d247abc2734086862c4"] = "bottle_inverted",
		["445f2dd2d733de04fb9f6feb0e2dae34"] = "null"
	};
	    
    private readonly VirtualMachine _vm;
    private MindMapNode? _currentNode;

    private readonly TableLayoutPanel _nodePropertiesPanel;
    private readonly TextBox _nameBox;
    private readonly ComboBox _logicMapNodeTypeComboBox;

    private readonly SplitContainer _contentSplitContainer;
    private readonly TableLayoutPanel _contentListPanel;
    private readonly FlowLayoutPanel _contentButtonPanel;
    private readonly ListBox _contentList;
    private readonly Button _addContentButton;
    private readonly Button _removeContentButton;

    private readonly TableLayoutPanel _contentDetailsPanel;
    private readonly TextBox _contentNameBox;
    private readonly ComboBox _contentTypeComboBox;
    private readonly Label _contentDescriptionPreview;
    private readonly ComboBox _contentPictureComboBox;
    private readonly Label _contentConditionPreview;
    private readonly ToolTip _toolTip;
    private MindMapNodeContent? _selectedContent;

    private bool _updatingPictureComboBox;
    
    public NodePropertiesPanel(VirtualMachine vm) {
        _vm = vm;
        Dock = DockStyle.Right;
        Width = 300;
        _toolTip = new ToolTip();
        var mainLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        Controls.Add(mainLayout);
        _nodePropertiesPanel = new TableLayoutPanel {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        _nodePropertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        _nodePropertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        var nameLabel = new Label { Text = "Name:", Anchor = AnchorStyles.Left, AutoSize = true };
        _nameBox = new TextBox { Dock = DockStyle.Fill };
        _nameBox.TextChanged += OnNameChanged;
        _nodePropertiesPanel.Controls.Add(nameLabel, 0, 0);
        _nodePropertiesPanel.Controls.Add(_nameBox, 1, 0);
        var typeLabel = new Label { Text = "Node Type:", Anchor = AnchorStyles.Left, AutoSize = true };
        _logicMapNodeTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _logicMapNodeTypeComboBox.DataSource = Enum.GetValues(typeof(LogicMapNodeType));
        _logicMapNodeTypeComboBox.SelectedIndexChanged += OnNodeTypeChanged;
        _nodePropertiesPanel.Controls.Add(typeLabel, 0, 1);
        _nodePropertiesPanel.Controls.Add(_logicMapNodeTypeComboBox, 1, 1);
        mainLayout.Controls.Add(_nodePropertiesPanel, 0, 0);
        _contentSplitContainer = new SplitContainer {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 120
        };
        _contentListPanel = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        _contentListPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contentListPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        _contentButtonPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 35,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        _addContentButton = new Button { Text = "Add Content", AutoSize = true, Height = 30 };
        _addContentButton.Click += OnAddContent;
        _removeContentButton = new Button { Text = "Remove Content", AutoSize = true, Height = 30 };
        _removeContentButton.Click += OnRemoveContent;
        _contentButtonPanel.Controls.Add(_addContentButton);
        _contentButtonPanel.Controls.Add(_removeContentButton);
        _contentList = new ListBox { Dock = DockStyle.Fill, Height = 20, FormattingEnabled = true, AutoSize = false};
        _contentList.Format += (_, e) => {
            if (e.ListItem is MindMapNodeContent content)
                e.Value = string.IsNullOrEmpty(content.Name) ? $"Content {content.Number}" : content.Name;
        };
        _contentList.SelectedIndexChanged += OnContentSelected;
        _contentListPanel.Controls.Add(_contentButtonPanel, 0, 0);
        _contentListPanel.Controls.Add(_contentList, 0, 1);
        _contentSplitContainer.Panel1.Controls.Add(_contentListPanel);
        _contentDetailsPanel = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5
        };
        _contentDetailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        _contentDetailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        _contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        _contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        _contentDetailsPanel.Controls.Add(
            new Label { Text = "Content Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        _contentNameBox = new TextBox { Dock = DockStyle.Fill };
        _contentNameBox.TextChanged += (_, _) => {
	        if (_selectedContent == null) return;
	        _selectedContent.Name = _contentNameBox.Text;
	        RefreshContentList();
        };
        _contentDetailsPanel.Controls.Add(_contentNameBox, 1, 0);
        _contentDetailsPanel.Controls.Add(
            new Label { Text = "Content Type:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        _contentTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _contentTypeComboBox.DataSource = Enum.GetValues(typeof(NodeContentType));
        _contentTypeComboBox.SelectedIndexChanged += (_, _) => {
	        if (_selectedContent == null || _contentTypeComboBox.SelectedItem is not NodeContentType type) return;
	        _selectedContent.ContentType = type;
        };
        _contentDetailsPanel.Controls.Add(_contentTypeComboBox, 1, 1);
        _contentDetailsPanel.Controls.Add(
            new Label { Text = "Description:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
        _contentDescriptionPreview = new Label
            { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 60 };
        _contentDescriptionPreview.DoubleClick += OnEditDescriptionClicked;
        _toolTip.SetToolTip(_contentDescriptionPreview, "Double-click to edit description");
        _contentDetailsPanel.Controls.Add(_contentDescriptionPreview, 1, 2);
        _contentDetailsPanel.Controls.Add(new Label { Text = "Picture:", Anchor = AnchorStyles.Left, AutoSize = true },
            0, 3);
        _contentPictureComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
	        FormattingEnabled = true  };
        _contentPictureComboBox.SelectedIndexChanged += OnContentPictureChanged;
        _contentPictureComboBox.Format += (_, e) => {
	        if (e.ListItem is Sample sample) 
		        e.Value = EngineIdToImageMap.TryGetValue(sample.EngineId, out var name) ? name : sample.EngineId;
        };
        _contentDetailsPanel.Controls.Add(_contentPictureComboBox, 1, 3);
        _contentDetailsPanel.Controls.Add(
            new Label { Text = "Condition:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
        _contentConditionPreview = new Label
            { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 60 };
        _contentConditionPreview.DoubleClick += OnEditConditionClicked;
        _toolTip.SetToolTip(_contentConditionPreview, "Double-click to edit condition");
        _contentDetailsPanel.Controls.Add(_contentConditionPreview, 1, 4);
        _contentSplitContainer.Panel2.Controls.Add(_contentDetailsPanel);
        mainLayout.Controls.Add(_contentSplitContainer, 0, 1);
        _nodePropertiesPanel.Enabled = false;
        _contentButtonPanel.Enabled = false;
        _contentDetailsPanel.Visible = false;
    }
    
	public void SetNode(MindMapNode? node) {
        _currentNode = node;
        _nodePropertiesPanel.Enabled = node != null;
        _contentButtonPanel.Enabled = node != null;
        if (node != null) {
            _nameBox.Text = node.Name;
            _logicMapNodeTypeComboBox.SelectedItem = node.LogicMapNodeType;
        } else {
            _nameBox.Text = "";
        }

        RefreshContentList();
        
        if (node?.Content.Count > 0)
	        _contentList.SelectedIndex = 0;
        else
	        ClearContentDetails();
        
        Invalidate();
    }

    private void RefreshContentList() {
        _contentList.Items.Clear();
        if (_currentNode == null) return;
        foreach (var content in _currentNode.Content)
            _contentList.Items.Add(content);
    }
    
    private void RefreshContentDetails() {
	    if (_selectedContent == null) return;
	    _contentDetailsPanel.Visible = true;
	    _contentNameBox.Text = _selectedContent.Name;
	    _contentTypeComboBox.SelectedItem = _selectedContent.ContentType;
	    _contentDescriptionPreview.Text = _selectedContent.ContentDescriptionText.GetText("english");
	    _contentConditionPreview.Text = PreviewHelper.Preview(_selectedContent.ContentCondition);

	    _updatingPictureComboBox = true;
	    try {
		    var samples = _vm.GetElementsByType<Sample>()
			    .Where(s => s.SampleType == SampleType.MindMapPicture)
			    .ToList();
            
		    _contentPictureComboBox.DataSource = null;
		    _contentPictureComboBox.DataSource = samples;

		    if (_selectedContent.ContentPicture == null) return;
		    var selectedSample = samples.FirstOrDefault(s => s.Id == _selectedContent.ContentPicture.Id);
		    _contentPictureComboBox.SelectedItem = selectedSample;
		    
	    } finally {
		    _updatingPictureComboBox = false;
	    }
    }

    private void OnContentPictureChanged(object? sender, EventArgs e) {
	    if (_updatingPictureComboBox) return;
	    if (_selectedContent == null || _contentPictureComboBox.SelectedItem is not Sample sample) return;
	    _selectedContent.ContentPicture = sample;
    }
    
    private void OnEditDescriptionClicked(object? sender, EventArgs e) {
        if (_selectedContent == null) return;
        var editor = new GameStringEditor(_selectedContent.ContentDescriptionText, _vm);
        if (editor.ShowDialog() == DialogResult.OK) {
            _contentDescriptionPreview.Text = _selectedContent.ContentDescriptionText.GetText("english");
        }
    }


    private void OnNameChanged(object? sender, EventArgs e) {
        if (_currentNode != null) {
            _currentNode.Name = _nameBox.Text;
            if (Parent is MindMapViewer viewer)
                viewer.RefreshView();
        }
    }

    private void OnNodeTypeChanged(object? sender, EventArgs e) {
        if (_currentNode != null && _logicMapNodeTypeComboBox.SelectedItem is LogicMapNodeType newType) {
            _currentNode.LogicMapNodeType = newType;
            if (Parent is MindMapViewer viewer)
                viewer.RefreshView();
        }
    }

    private void OnAddContent(object? sender, EventArgs e) {
        if (_currentNode == null) return;
        var content = VmElement.CreateDefault<MindMapNodeContent>(_vm, _currentNode);
        _currentNode.Content.Add(content);
        RecalculateContentNumbers();
        RefreshContentList();
    }

    private void OnRemoveContent(object? sender, EventArgs e) {
        if (_currentNode == null || _contentList.SelectedItem is not MindMapNodeContent content)
            return;
        _currentNode.Content.Remove(content);
        _vm.RemoveElement(content);
        RecalculateContentNumbers();
        RefreshContentList();
        ClearContentDetails();
    }

    private void RecalculateContentNumbers() {
        if (_currentNode == null) return;
        for (var i = 0; i < _currentNode.Content.Count; i++)
            _currentNode.Content[i].Number = i;
    }

    private void OnContentSelected(object? sender, EventArgs e) {
        if (_contentList.SelectedItem is MindMapNodeContent content) {
            _selectedContent = content;
            RefreshContentDetails();
        } else {
            _selectedContent = null;
            ClearContentDetails();
        }
    }

    private void ClearContentDetails() {
        _contentDetailsPanel.Visible = false;
        _contentNameBox.Text = "";
        _contentTypeComboBox.SelectedIndex = -1;
        _contentDescriptionPreview.Text = "";
        _contentPictureComboBox.DataSource = null;
        _contentConditionPreview.Text = "";
    }

    private void OnEditConditionClicked(object? sender, EventArgs e) {
        if (_selectedContent == null) return;
        using var editor = new ConditionEditorForm(_vm, _selectedContent.ContentCondition, new(_currentNode!));
        if (editor.ShowDialog() == DialogResult.OK)
        {
            _contentConditionPreview.Text = PreviewHelper.Preview(_selectedContent.ContentCondition);
        }
    }
}