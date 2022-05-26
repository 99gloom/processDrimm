class processFinalFilter:
    def __init__(self, sp, blockDir, resultDir, chr_shape):
        self.sp = sp
        self.blockDir = blockDir
        self.resultDir = resultDir
        self.chr_shape = chr_shape


    def filterBlock(self, blockDir, resultDir, sp):

        # 获取synteny所有匹配成功的block以及次数
        synteny_block_position = {}
        with open(resultDir  + '/' + sp + '.synteny' , 'r') as sf:
            for i in sf:
                block_info = i
                block_info = block_info.split(' ')[0].split(':')
                if block_info[0] not in synteny_block_position.keys():
                    synteny_block_position[block_info[0]] = []
                synteny_block_position[block_info[0]].append(int(block_info[1]))

        # 获取所有block序列
        block_sequence = []
        with open(blockDir + '/' + sp + '.block', 'r') as inf:
            for i in inf:
                block_info = i
                block_info = block_info.rstrip('\n').rstrip()
                block_info = block_info.split(' ')[1:]
                block_sequence.append(block_info)

        # 初始化block出现序列为0
        raw_block_position = {}
        for i in block_sequence:
            for j in i:
                if j.startswith('-'):
                    j = j[1:]
                if j not in raw_block_position.keys():
                    raw_block_position[j] = 0


        with open(resultDir + '/' + sp + '.final.block', 'w') as outf:
            for i in block_sequence:
                if self.chr_shape == 's' or self.chr_shape == 'S':
                    outf.write('s ')
                else:
                    outf.write('c ')

                for j in i:
                    if j.startswith('-'):
                        block = j[1:]
                    else:
                        block = j

                    raw_block_position[block] += 1
                    if raw_block_position[block] in synteny_block_position[block]:
                        outf.write(j + ' ')
                    # else:
                    #     print(block+':'+str(raw_block_position[block]) + 'drop!')
                outf.write('\n')
        return

    def excute(self):

        for i in self.sp:
            self.filterBlock(self.blockDir, self.resultDir, i)


# if __name__ == '__main__':
#     sp = ['Telongatum']
#     c = processFinalFilter(sp,'C:/Users/DELL/OneDrive/IAGS_input_builder/IAGS_input_builder/output_filterTwice/temp/RawBlocks',
#                            'C:/Users/DELL/OneDrive/IAGS_input_builder/IAGS_input_builder/output_filterTwice/result','s')
#     c.excute()