# RE4-UHD-SCENARIO-SMD-TOOL
Extract and repack RE4 UHD/PS4/NS/X360/PS3 Scenario SMD file

**Translate from Portuguese Brazil**

Programa destinado a extrair e recompactar os cenários usando somente um arquivo .OBJ;
<br>Nota: A versão de UHD é um executável, a versão de PS4/NS é outro executável, e a versão de X360/PS3 também é outro executável;

## Tutorial
Veja abaixo tutoriais em português de como usar a tool:
<br>[RE4 UHD Tutorial Editando Scenarios SMD](https://jaderlink.blogspot.com/2023/11/RE4-UHD-TUTORIAL-SCENARIO-SMD.html)
<br>[RE4 UHD Tutorial Editando r100.SMD](https://jaderlink.blogspot.com/2023/11/RE4-UHD-TUTORIAL-R100-SMD.html)

## Updates

**Update: B.1.2.3**
<br>Adicionado suporte para as versões big endians PS3 e X360;
<br>Aviso: não misture os Bin/Tpl dessa versão com as das outras versões, pois vai dar erro no jogo e o programa não vai mais conseguir extrair o SMD;

**Update: B.1.2.2**
<br> Melhoria: melhorado a velocidade do repack, agora é muito rápido fazer o repack.
<br> Correção: corrigido o "width X height" no TPL que estava invertido nas versões anteriores. A ordem correta no arquivo é "height X width";
<br> E foram feitas melhorias gerais no código;

**Update: B.1.2.0**
<br>Adicionado suporte para as versões de PS4 e NS;
<br>Aviso: não misture os Bin/Tpl da versão UHD com a versão de PS4/NS, pois vai dar erro no jogo e o programa não vai mais conseguir extrair o SMD;

**Update: B.1.1.0**
<br>Adicionado o campo "EnableDinamicVertexColor" no arquivo ".idxuhdscenario", que ao ativar será colocado somente o conteúdo de "VertexColor" para os BINs que realmente tenham cor por vértice.
<br>Agora, numerações de BIN puladas serão preenchidas com um bin de 0 (zero) materiais.
<br>Nota: SMD_Entry pulados, ainda vai ser atribuído o BIN de ID 0;
<br>Agora, ao fazer repack com ".idxuhdscenario" o arquivo ".idxuhdtpl" será ignorado. Para usá-lo, você deve ativar a variável "UseIdxUhdTpl" dentro do ".idxuhdscenario";

**Update: B.1.0.09**
<br>Agora, ao extrair o arquivo .bin as "normals" serão normalizadas, em vez de ser dividida por um valor padrão, então agora é possível extrair os arquivos .bin gerados pela tool do percia sem erros.
<br> Ao fazer repack as normals do arquivo .obj serão normalizadas para evitar erros.
<br> O programa, ao gerar o arquivo .obj, não terá mais os zeros não significativos dos números, mudança feita para gerar arquivos menores.

**Update: B.1.0.0.8**
<br>Arrumado bug ao carregar o arquivo .idxmaterial;

**Update: B.1.0.0.7**
<br>Agora o programa é compatível em extrair e criar .SMD com arquivos .BIN acima do limite de vértices;
<br>Atenção: Os .BIN com vértices acima do limite só funcionam corretamente se eles forem usados dentro de arquivos Scenario .SMD;
<br>O uso acima do limite do vértice é permitido, mas use com cuidado.
<br>Em outras situações, o limite ainda é valido;

**Update: B.1.0.0.6**
<br>Corrigido bug no qual não era rotacionado as normals dos modelos que têm rotação,
então, caso esteja usando um .obj de versões anteriores, recalcule as normals;
<br>Corrigido um bug que, ao extrair as cores de vértices, estava sendo colocado de maneira errada no arquivo obj;

**Update: B.1.0.0.5**
<br>Corrigido bug no qual o arquivo MTL com PACK_ID com IDs que continham letras, as letras não eram consideradas.

**Update: B.1.0.0.4**
<br>Corrido erro, ao ter material sem a textura principal "map_Kd", será preenchido como Pack ID00000000 e texture ID 000;
<br> Agora, caso a quantidade de vértices for superior ao limite do arquivo, o programa vai avisar. (Não será criado o arquivo SMD);

**Update B.1.0.0.3**
<br>Corrigido bug que deformava a malha do modelo 3d, estava sendo criado faces do tipo "quad" de maneira errada; 

**Update B.1.0.0.2**
<br>Adicionado compatibilidade com outros editores 3D que não suportam caracteres especiais #: como, por exemplo, o 3dsMax;
<br> Adicionado também uma verificação no nome dos grupos, então caso esteja errado o nome, o programa avisa-rá;
<br> Os arquivos da versão anterior são compatíveis com essa versão;

**Update B.1.0.0.1**
<br> * Adicionado suporte para o **R100**, agora você pode extrair esse cenario dividido em 7 SMD, em um único arquivo .obj, use "R100.r100extract"; (Veja mais formações mais abaixo);
<br> * Adicionado verificação do "magic" do arquivo .Smd;
<br> * Nos arquivos ".idxuhdscenario" e ".idxuhdsmd" adicionados os campos "Magic" e "ExtraParameterAmount", no qual só vão aparecer caso forem usados.
<br> * O "Magic" padrão é o 0x0040;
<br> * Corrigido a extração do campo "vertexColors", no arquivo .obj;
<br> * Os arquivos da versão anterior são compatíveis com essa versão.

## RE4_\*\*_SCENARIO_SMD_TOOL.exe

Programa destinado a extrair e reempacotar os arquivos de cenário SMD do RE4 UHD/PS4/NS/X360/PS3;

## Extract:

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL Extract all scenario SMD.bat"
<br>Nesse exemplo, vou usar o arquivo: r204_004.SMD
<br>Ao extrair, serão gerados os arquivos:

* "r204_004.scenario.idxuhdscenario" // arquivo importante de configurações, para o repack usando o .obj;
* "r204_004.scenario.idxuhdsmd" //  arquivo importante de configurações, para o repack usando os arquivos .bin;
* "r204_004.scenario.obj" // conteúdo de todo o cenário, esse é o arquivo que você vai editar;
* "r204_004.scenario.mtl" // arquivo que acompanha o .obj;
* "r204_004.scenario.idxmaterial" // conteúdo dos materiais (alternativa ao uso do .mtl);
* "r204_004.scenario.idxuhdtpl" // representa o arquivo .tpl, presente no .smd;
* "r204_004_BIN\" //pasta contendo os arquivos .bin e o .tpl;

## Repack:

Existem duas maneiras de fazer o repack.
* Usando o arquivo .idxuhdscenario, o repack será feito usando o arquivo .obj;
* Usando o arquivo .idxuhdsmd, o repack será feito com os arquivos .bin da pasta "r204_004_BIN";

## Repack com .idxuhdscenario

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL Repack all with idxuhdscenario.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxuhdscenario";
<br> que vai requisitar os arquivos:
* r204_004.scenario.obj (obrigatório);
* r204_004.scenario.mtl OU r204_004.scenario.idxmaterial + r204_004.scenario.idxuhdtpl;

Ao fazer o repack, serão gerados os arquivos:
* "r204_004.SMD" (esse é o arquivo para ser colocado no .udas);
* "r204_004.scenario.Repack.idxmaterial";
* "r204_004.scenario.Repack.idxuhdtpl";
* "r204_004.scenario.Repack.idxuhdsmd";
* "r204_004_BIN\" //pasta contendo os novos arquivos .bin e o novo .tpl; (aviso: ele sobrescreve os arquivos);

## Repack com .idxuhdsmd

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL Repack all with idxuhdsmd.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxuhdsmd";
<br> Que vai requisitar os arquivos:
<br>-- os arquivos .bin e .tpl da pasta "r204_004_BIN";

Ao fazer o repack, será gerado o arquivo:
<br>-- "r204_004.SMD" (esse é o arquivo para ser colocado no .udas);

Nota: esse é o método antigo, no qual se edita os bin individualmente, porém o repack com .idxuhdscenario cria novos bin modificados, e um novo .idxuhdsmd, no qual pode ser usado para fazer esse repack; essa opção é para caso você queira colocar um .bin no .smd que o programa não consiga criar.

## Sobre r204_004.scenario.obj

Esse arquivo é onde está todo o cenário, nele os arquivos BIN são separados por grupos, no qual a nomenclatura deve ser respeitada:
<br> Exemplo:
<br> UHDSCENARIO#SMD_000#SMX_000#TYPE_08#BIN_000#
<br> UHDSCENARIO#SMD_001#SMX_001#TYPE_08#BIN_001#

Sendo:
* É obrigatório o nome do grupo começar com "UHDSCENARIO", e ser dividido por #
* A ordem dos campos não pode ser mudada;
* SMD_000 esse é o ID da posição da Entry/Line no .SMD, a numeração é em decimal;
* SMX_000 esse é o ID do SMX, veja o arquivo .SMX,  a numeração é em decimal;
* TYPE_08 esse é uma flag bit a bit, a numeração é em hexadecimal;
* BIN_000 esse é o id do bin que será usado. Para bin repetidos, será considerado somente o primeiro, (o próximo, com o mesmo id, será usado o mesmo modelo que do primeiro).
* O nome do grupo deve terminar com # (pois após salvo o arquivo, o Blender coloca mais texto no final do nome do grupo);

----> No Update B.1.0.0.2, o nome dos objetos/grupos também pode ser:
<br> UHDSCENARIO\_SMD\_000\_SMX\_000\_TYPE\_08\_BIN\_000\_
<br> UHDSCENARIO\_SMD\_001\_SMX\_001\_TYPE\_08\_BIN\_001\_

----> Sobre verificações de grupos:
<br> * No Repack se ao lado direito do nome do grupo aparecer o texto "The group name is wrong;", significa que o nome do grupo está errado, e o seu arquivo SMD vai ficar errado;
<br> * E se ao lado direito aparecer "Warning: Group not used;" esse grupo está sendo ignorado pelo meu programa. Caso, na verdade, você gostaria de usá-lo, você deve arrumar o nome do grupo;


**Editando o arquivo .obj no Blender**
<br>No importador de .obj marque a caixa "Split By Group" que está no lado direito da tela.
<br>Com o arquivo importado, cada objeto representa um arquivo .BIN
<br>![Groups](UhdGroups.png)
<br>Nota: caso você tenha problema com texturas transparentes ficando pretas, use esse plugin: (**[link](https://github.com/JADERLINK/Blender_Transparency_Fix_Plugin)**) 

**Ao salvar o arquivo**
<br>Marque as caixas "Triangulated Mesh" e "Object Groups" e "Colors".
<br> no arquivo .obj o nome dos grupos vai ficar com "_Mesh" no final do nome (por isso, no editor, termina o nome do grupo com # para evitar problemas);

## Sobre .idxuhdscenario / .idxuhdsmd
Segue abaixo a lista de comandos mais importantes presente no arquivo:

* SmdAmount:106 // representa a quantidade de Entry/Line no .Smd, coloque a mesma quantidade que você colocou de entry no arquivo .obj;
* SmdFileName:r204_004.SMD // esse é o nome do arquivo Smd que será gerado;
* BinFolder:r204_004_BIN // esse é o nome da pasta onde será salvo/estão os arquivos .bin e o .tpl;
UseIdxMaterial:false // Caso ativado, será o usado o arquivo .idxmaterial e .idxuhdtpl ao invés do .mtl para fazer o repack (campo somente no idxuhdscenario);
* UseIdxUhdTpl:false // usa o conteúdo de UseIdxUhdTpl para forçar a ordem dos Ids dos TplEntry ao fazer o repack (campo somente no idxuhdscenario);
* EnableVertexColor:false // Se ativado, cria os bins com o campo de "Vertex Color", mas o .obj não tem um suporte adequado para isso (campo somente no idxuhdscenario);
* EnableDinamicVertexColor:true // o mesmo que o de cima, porém só vai criar o campo "Vertex Color" somente para os bins que realmente têm pintura de vertices. (campo somente no idxuhdscenario);
* BinAmount:106 // quantidade de arquivos bins que serão colocados no .smd (campo somente no idxuhdsmd);
* Os outros comandos que começam com números são autodescritivos (o número é o ID do Smd)
* o campo "_position*" é dividido por 100, em relação ao valor original;
* o campo "_objectStatus" refere-se ao campo "TYPE" no .obj;

# sobre .idxmaterial e .idxuhdtpl
Veja sobre em [RE4-UHD-BIN-TOOL](https://github.com/JADERLINK/RE4-UHD-BIN-TOOL);


# sobre .r100extract e r100extract
Para extrair o cenário, coloque os arquivos .Smd necessários ao lado de .r100extract;
<br> Nota: No tópico **tutorial** tem um tutorial sobre como editar o r100;

## Código de terceiro:

[ObjLoader by chrisjansson](https://github.com/chrisjansson/ObjLoader):
Encontra-se no RE4_UHD_SCENARIO_SMD_TOOL\\CjClutter.ObjLoader.Loader, código modificado, as modificações podem ser vistas aqui: [link](https://github.com/JADERLINK/ObjLoader).

**At.te: JADERLINK**
<br>Thanks to \"mariokart64n\" and \"CodeMan02Fr\"
<br>Material information by \"Albert\"
<br>2025-03-23