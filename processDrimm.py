import os
from pathlib import Path
from utils import processLCSAndFirstFilter as plff
from utils import processFinalFilter as pff

def readSequence(file):
    sequence = []
    with open(file,'r') as f:
        while True:
            line = f.readline()[:-2]
            if not line:
                break
            itemset = line.split(' ')
            sequence.append(itemset)
    return sequence

def syntenyDict(file):
    syntenyDict = {}
    with open(file) as sf:
        while True:
            line = sf.readline()[:-2]
            if not line:
                break
            itemset = line.split(' ')
            header = itemset[0].split(':')
            syntenyDict[header[0]] = itemset[1:]
    return syntenyDict

# 用来处理drimm输出得到各个物种的输入
block_file = './example/drimm/blocks.txt'
synteny_file = './example/drimm/synteny.txt'
outdir = './example'
chr_number = [5,10,12,10,7]
sp_list = ['Brachy','Maize','Rice','Sorghum','Telongatum']
target_rate = '2:4:2:2:2'


if (not Path(outdir + '/result').exists()):
    os.makedirs(outdir + '/result')
if (not Path(outdir + '/blockFiles').exists()):
    os.makedirs(outdir + '/blockFiles')
drimm_split_blocks_dir = outdir + '/drimm'
raw_block_dir = outdir + '/blockFiles'
result_dir = outdir + '/result'

sequence = readSequence(block_file)
sp_sequences = []
last = 0
for i in range(len(chr_number)):
    sp_sequences.append(sequence[last:last+chr_number[i]])
    last += chr_number[i]

for i in range(len(sp_sequences)):
    outfile = drimm_split_blocks_dir + '/' + sp_list[i] + '.block'
    outfile = open(outfile,'w')
    for j in sp_sequences[i]:
        outfile.write('s ')
        for k in j:
            if k.startswith('-'):
                block = k[1:]
            else:
                block = k
        outfile.write('\n')
    outfile.close()

processLCSAndFirstFilter = plff.processLCSAndFirstFilter(result_dir, raw_block_dir, target_rate,
                                                         drimm_split_blocks_dir, outdir, drimm_split_blocks_dir + '/synteny.txt',
                                                         sp_list, 's')
processLCSAndFirstFilter.excute()

processFinalFilter = pff.processFinalFilter(sp_list, raw_block_dir, result_dir, 's')
processFinalFilter.excute()







# block_rate_dir = {}
# for i in sp_sequences:
#     for j in i:
#         for k in j:
#             block = ''
#             if k.startswith('-'):
#                 block = k[1:]
#             else:
#                 block = k
#             if block not in block_rate_dir.keys():
#                 rate_list = []
#                 for l in chr_number:
#                     rate_list.append(0)
#                 block_rate_dir[block] = rate_list
#
# for i in range(len(sp_sequences)):
#     for j in sp_sequences[i]:
#         for k in j:
#             block = ''
#             if k.startswith('-'):
#                 block = k[1:]
#             else:
#                 block = k
#             block_rate_dir[block][i] += 1
# save_block = []
# for i in block_rate_dir.keys():
#     rate = ''
#     for j in block_rate_dir[i]:
#         rate += str(j) + ':'
#     rate = rate[:-1]
#     if rate == target_rate:
#         save_block.append(i)
#
# synteny = syntenyDict(synteny_file)
# save_block_filter = []
# for i in save_block:
#     save_block_filter.append(i)
# # 输出过滤情况
# print(len(save_block))
# print(len(save_block_filter))
#


