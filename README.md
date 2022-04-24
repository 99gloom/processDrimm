# To get IAGS input (windows)

The blocks file used by IAGS can be generated by DRIMM-synteny.

[Chinese version](./README_zh.md)

## 1. Generate the input file for DRIMM-synteny (processOrthofinder.py)

```python
dir = './processDRIMM/example/'

sp = ['Brachy','Maize','Rice','Sorghum','Telongatum']
gff_list = ['Brachy.gff','Maize.gff','Rice.gff','Sorghum.gff','Telongatum.gff']
sp_ratio = [2,4,2,2,2]
```

+ dir: The directory of all species gff files (the gff format is consistent with [MCScanX](https://github.com/wyp1125/MCScanx))
+ sp: Species names
+ Orthogroups.tsv: The output file of [OrthoFinder](https://github.com/davidemms/OrthoFinder)
+ sp_ratio: Target copy number for species, e.g. one for no WGD event and two for one WGD event


According to the target copy number of the species, processOrthofinder.py will filter the homologous genes that exceed the target copy number and obtain the gene order sequences used to prepare the synteny blocks. The output are .sequence files. Then, all the .sequence files need to be merged into drimm.sequence file which is the input of DRIMM-synteny.

## 2.Running  DRIMM-synteny (drimm.exe)

Since the version of dotnet may be different in each PC, we recommend recompiling to generate the new .exe file.

+ Using `dotnet publish -c Release -r win-x64` in  ./drimm to obtain drimm.exe (in ./drimm/drimm/bin/Release/netcoreapp3.1/win-x64)

Running drimm.exe

![DRIMM-synteny interface](https://s1.328888.xyz/2022/04/23/2R9HB.png)

The input of DRIMM-synteny:
+ The path of drimm.sequence
+ The output directory
+ cycleLengthThreshold controls the continuity of synteny blocks (default parameter is 20)
+ dustThreshold controls the maximum homologous gene diversity. The number of homologous genes exceeding dustThreshold will be filtered. For above example, target copy numbers are 2,4,2,2,2 in each species, then the dustThreshold is 13 (2+4+2+2+2+1)

The output of DRIMM-synteny
+ synteny.txt
```
0:1 2147483647 -1 
1:1 8997 
2:1 -2147482956 -2147482957 -2147482958 -2147482959 -2147482960 -2147482961 -2147482962 -2147482963 -2147482964 -2147482965 -2147482966 -2147482967 -2147482968 -2147482969 -2147482970 -2147482971 -2147482972 -2147482973 -2147482974 -2147482975 -2147482976 
3:1 13651 10527 2147481608 
4:1 10498 6507 
5:1 7035 2147482195 3426 -2147482913 -2147482912 -2147482911 -2147482910 -2147482909 -2147482908 -2147482907 -2147482906 -2147482905 -2147482904 -2147482903 -2147482902 -2147482901 -2147482900 -2147482899 -2147482898 -2147482897 -2147482896 -2147482895 -2147482894 -2147482893 
6:1 10464 
7:1 2147480311 
8:1 8926 8925 
9:1 6472 2147482102 992 
10:1 2147481278 13638 2147479645 128 
11:1 8808 13137 
12:1 2147481394 -2147483438 -2147483437 -2147483436 -2147483435 -2147483434 -2147483433 -2147483432 -2147483431 -2147483430 -2147483429 -2147483428 -2147483427 -2147483426 -2147483425 -2147483424 -2147483423 -2147483422 -2147483421 -2147483420 -2147483419 -2147483418 2147483493 10949 
13:1 2147478991 

#The first column is the ID of the block
#The second column (after the colon) is the number of block occurrences in all species
#The third column and later are the IDs of the homologous genes
```
+ blocks.txt
```
-261 -434 -774 -1204 -1358 -1227 -1393 479 -1227 -1393 -1031 -1159 676 -971 -561 921 911 782 -918 1317 -827 -518 -897 1324 -1381 1311 -1377 -1310 -851 300 -860 -1287 -1438 -1430 1421 -843 -430 381 668 421 321 539 415 -472 -646 -475 -1001 -1303 -1072 -796 -1075 -1081 -1271 1085 -1091 1272 1051 -1282 -1379 1291 -1150 629 -1173 -934 -1335 -958 -923 1220 -1066 -1062 -768 1206 -607 -1054 -1230 -1048 755 -1044 -729 -1007 -658 -998 -815 -1035 -605 -638 -1023 -1012 1056 -713 -1196 -1176 686 -1100 -662 -981 -368 -1200 -285 -1125 -777 -1170 -641 -865 -1223 -848 -687 -951 -681 -950 -943 -714 -588 -698 -966 -647 -957 -810 -576 -899 880 -731 885 -580 -920 -914 -913 -912 -570 764 -1437 -783 916 -776 -1437 -783 916 590 1258 -878 -832 -792 -230 -348 -890 1225 -926 -1332 963 1328 936 -1323 -975 574 423 -245 -45 226 44 
-716 937 -571 -942 -1312 -944 -602 -947 -675 -949 -876 1373 905 -1337 -856 -665 622 603 -855 337 -873 -594 -846 -864 -733 -730 -1380 -1369 -869 -601 -215 733 -850 -980 769 -1256 -779 575 -1380 -1369 -450 -840 -1367 -1126 -797 -1128 1130 459 -1405 -1117 1187 706 -1195 717 -584 -398 1253 -586 1342 -1156 -591 696 -1408 1357 -1314 1039 1359 -1268 316 320 -417 -313 473 -653 403 -645 1422 -990 1030 -1302 -1006 -1040 999 1289 1397 -1288 -1078 -1286 395 965 454 -1089 -745 -1090 -742 -1093 790 -1095 -1067 -1045 -572 784 941 -781 -1053 -789 -1058 -462 -1063 1232 1064 1350 -1402 -1285 -1284 -1065 -762 -1061 -1060 -766 -1059 -1057 -771 -772 1445 -1415 -1392 -1055 -1280 1392 1415 788 -788 -1445 -1047 -1277 -436 597 -1276 -734 -739 -1092 -482 -480 1088 -748 -750 -1087 -1273 -1067 1350 -1402 -255 925 -16 1095 -790 1093 742 1090 745 1089 -454 -965 -1286 380 1078 1288 -1397 -1289 -999 1040 1006 1302 1030 990 -1422 645 199 1388 290 653 17 
-1024 -1304 1004 1203 -239 1041 -997 496 -812 -995 -994 -993 -821 1298 -1297 -991 988 -1296 -986 -499 -985 461 749 1008 -723 -1009 -1011 829 583 1187 706 -1195 717 -584 -398 1253 -586 1342 204 474 1422 696 -1408 1357 -1314 1039 1359 -1268 -643 -655 1038 1290 1037 -650 582 1099 -630 1022 996 806 -540 820 -1019 -625 -1388 -1017 1344 1016 1216 956 1321 1013 -617 1020 287 1076 -286 1129 1174 1189 -1245 -1363 -1166 -1164 801 -1161 -1397 -1157 -722 -1154 1348 1248 -1175 -1178 -715 1172 -716 937 571 -942 -1312 -944 675 947 -949 -876 200 873 440 855 622 603 665 856 1337 -905 -1373 431 -594 -846 840 733 -850 -864 -601 -730 -1380 -1369 -869 455 -575 779 1256 -769 980 -1367 -1126 -797 -1128 537 1130 -597 389 -1276 -734 -739 -1092 480 482 1088 -748 -750 -1087 -1273 -925 -1183 874 1235 -688 -674 -1149 -680 394 274 
-1250 1116 1327 -1115 -791 -1113 -1266 -1112 -679 1111 1265 -22 1217 -305 1231 1106 -1105 -1262 1005 719 1073 847 1050 1046 1251 -1101 -1375 -1122 -1307 -1146 1259 -697 -1145 -657 -644 1182 628 627 -1257 -1141 -1139 -564 -1137 807 -566 1136 1267 -1132 1293 -535 798 -419 844 -830 -754 -1103 -1107 -743 -741 1110 -1120 -1179 -1441 -1152 868 866 894 1281 297 -21 
-21 -1357 1408 770 1165 -894 -866 1441 868 1152 1441 1179 -1120 -1110 741 743 1107 505 -409 844 -830 -754 -1103 505 -409 577 311 533 577 -535 798 -1293 1132 -1267 -1136 807 -566 1137 -608 -1139 -564 -608 1139 1141 1257 -627 -628 -1182 644 -697 -1145 -657 -1259 1146 1307 1122 1375 1101 -1251 -1046 -1050 467 -1020 617 -1013 -1321 -956 -1216 -1016 -1344 1017 1388 625 1019 -820 540 -806 -996 -203 -1022 630 -1099 1099 -1037 -1290 -1038 655 643 -1172 715 1178 1175 -1248 -1348 1154 -722 1157 1189 -631 -611 -862 1032 -689 606 -700 -1438 -1430 1421 490 843 -1421 1430 1438 1287 860 445 851 1310 1377 -1311 1381 -1324 897 238 921 911 782 -918 1317 971 -676 1159 1031 1393 1227 1358 1204 -1393 479 1358 1204 774 434 261 
-423 -574 975 1323 -936 -1328 -963 1332 926 -1225 890 -842 -590 418 -740 1437 792 832 878 -1258 -590 -916 783 1437 -764 570 912 -914 -913 920 880 -731 885 193 899 576 -588 698 -413 -966 -647 -957 -810 714 -848 950 681 951 -943 579 1223 865 -641 1170 777 1125 593 -1200 981 662 1100 -686 1176 1196 713 -1056 1012 1023 -301 -605 -638 729 -658 -998 815 -1035 -1007 1044 -755 1048 1230 1054 607 -1206 768 1062 1066 -1220 923 958 1335 934 1173 -629 1150 -1291 1379 1282 -1051 -1272 1091 -1085 1271 1081 1075 796 1072 1303 1001 -415 -539 -321 -421 -668 -381 430 
-933 -932 721 -704 -702 -940 -1319 -954 -695 -694 955 -1333 -976 -616 -974 -623 849 1162 1322 1079 973 -528 -875 1180 -621 -1353 -618 -969 893 -527 -1279 928 838 364 -526 854 523 -420 -189 -442 545 1398 -959 -362 -1398 1080 1077 1002 -339 -929 1094 898 1238 -1427 -1331 726 -895 1097 -517 -886 -1345 1383 642 889 881 879 1131 -673 -887 -209 900 809 -902 1123 922 -1346 -919 -917 -1340 -220 -1297 -991 988 -1296 -986 -1011 985 1009 723 -812 -361 -583 -829 461 749 1008 997 -234 994 995 -821 1298 867 549 463 786 232 -699 -904 243 -1041 492 -632 -1203 -1004 1304 1024 800 516 898 -426 282 1240 
93 -186 -393 -562 -1250 1116 1327 -1115 -791 -1113 -1266 -1112 -679 1111 1265 1217 560 1231 1106 -1105 -1262 1005 719 1073 847 1076 514 1129 318 1174 1161 -801 1164 1166 1363 1245 -841 -559 -336 -1363 -198 -811 932 933 721 -704 695 954 1319 -955 694 -702 -940 -1333 -976 -616 -974 -623 849 1162 1322 1079 973 969 -149 -1180 875 893 838 -1279 -527 -1279 928 526 456 319 -382 -786 -463 -549 -867 1340 917 919 1346 -922 1123 900 809 -902 511 1131 -673 -887 509 -879 -881 -889 -642 -1383 1345 -263 -1383 1345 886 347 258 345 1097 -345 895 -726 1331 1427 -1238 633 1034 443 344 727 -968 1329 -972 1010 

#Synteny blocks for species. The sign indicates direction.
```
## 3.Generate IAGS input (processDrimm.py)
```python
block_file = './processDRIMM/example/drimm/blocks.txt'
synteny_file = './processDRIMM/example/drimm/synteny.txt'
outdir = './processDRIMM/example/drimm/'
chr_number = [5,10,12,10,7]
sp_list = ['Brachy','Maize','Rice','Sorghum','Telongatum']
target_rate = '2:4:2:2:2'
```

+ block_file: The path of block.txt
+ synteny_file: The path of synteny.txt
+ outdir: The output directory
+ chr_number: The number of chromosomes in each species
+ sp_list: All species name
+ target_rate: The target copy number of each species

processDrimm.py splits blocks.txt file into each species and filters synteny blocks whose the number of block occurrences not equal to the target copy number in each species.

```
s -87 -86 -85 -122 83 -123 -124 125 106 109 -108 
s -156 -157 158 -155 -154 53 54 -152 -78 -77 128 -130 132 133 
s 134 143 60 -142 -58 -144 -59 -50 52 -153 -126 -89 90 119 81 -121 
s 33 -32 -169 -167 -168 166 -93 95 96 -97 101 100 -114 113 -111 
s -1 2 -7 -5 -3 8 -20 24 22 -21 -12 9 31 26 28 
```
This format could be IAGS input format and the s represents a linear chromosome.

## DRIMM-synteny


[DRIMM-Synteny: decomposing genomes into evolutionary conserved segments ](https://academic.oup.com/bioinformatics/article/26/20/2509/193644?login=false)

 

[DRIMM - Duplications and Rearrangements In Multiple Mammals](http://bix.ucsd.edu/projects/drimm/)
