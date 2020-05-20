import AElf from 'aelf-sdk';

const { sha256 } = AElf.utils;

const wallet = AElf.wallet.createNewWallet();

const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));


async function pollMining(txId, times = 0, delay = 2000, timeLimit = 10) {
    const currentTime = times + 1;
    await new Promise(resolve => {
        setTimeout(() => {
            resolve();
        }, delay);
    });
    let tx;
    try {
        tx = await aelf.chain.getTxResult(txId);
    } catch (e) {
        if (e.Status) {
            return e;
        }
        throw new Error('Network Error');
    }
    if (tx.Status === 'PENDING' && currentTime <= timeLimit) {
        const result = await pollMining(txId, currentTime, delay, timeLimit);
        return result;
    }
    if (tx.Status === 'PENDING' && currentTime > timeLimit) {
        return tx;
    }
    if (tx.Status === 'MINED') {
        return tx;
    }
    return tx;
}

let contract = {};
let genesisContract = null;
async function getContract(name, walletInstance) { // 异步函数，使用js-sdk获得合同对象:
    if (!genesisContract) {
        const chainStatus = await aelf.chain.getChainStatus(); // 获取创世合约的地址
        if (!chainStatus) {
            throw new Error('Error occurred when getting chain status');
        }
        genesisContract = await aelf.chain.contractAt(chainStatus.GenesisContractAddress, walletInstance); // 构建创世合约的实例
    }
    if (!contract[name]) {
        const address = await genesisContract.GetContractAddressByName.call(sha256(name)); // 用创世合约通过名字获得调用合约的地址
        contract = {
            ...contract,
            [name]: await aelf.chain.contractAt(address, walletInstance) // 构建调用合约的实例
        };
    }
    return contract[name];
}

function initDomEvent() { // 获取元素
    const refresh = document.getElementById('refresh');
    const chainInfo = document.getElementById('chaininfo');
    const greet = document.getElementById('greet');
    const greetResponse = document.getElementById('greetResponse');
    const greetToButton = document.getElementById('greetToButton');
    const getGreeted = document.getElementById('getGreeted');

    refresh.onclick = () => {
        aelf.chain.getChainStatus()
            .then(res => {
                if (!res) {
                    throw new Error('Error occurred when getting chain status');
                }
                chainInfo.innerHTML = JSON.stringify(res, undefined, 2);
            })
            .catch(err => {
                console.log(err);
            });
    };

    greet.onclick = () => {
        getContract('AElf.ContractNames.Greeter', wallet)
            .then(greeterContract => greeterContract.Greet.call())
            .then(ret => {
                greetResponse.innerHTML = JSON.stringify(ret, null, 2);
            })
            .catch(err => {
                console.log(err);
            });
    };

    greetToButton.onclick = () => {
        const nameToGreet = document.getElementById('nameToGreet');
        const greetToResponse = document.getElementById('greetToResponse');
        // 检索调用合约的实例,然后调用它的函数,函数会给节点发交易，交易等待挖矿完成被记账；完成后，将使用ReadableReturnValue来查看结果。
        getContract('AElf.ContractNames.Greeter', wallet)
            .then(greeterContract => greeterContract.GreetTo({
                value: nameToGreet.value
            }))
            .then(tx => pollMining(tx.TransactionId))
            .then(ret => {
                greetToResponse.innerHTML = ret.ReadableReturnValue;
            })
            .catch(err => {
                console.log(err);
            });
    };

    getGreeted.onclick = () => {
        const greeted = document.getElementById('greeted');
        // .call指出一个只读操作就是调用一个view方法
        getContract('AElf.ContractNames.Greeter', wallet)
            .then(greeterContract => greeterContract.GetGreetedList.call())
            .then(ret => {
                greeted.innerHTML = JSON.stringify(ret, null, 2);
            })
            .catch(err => {
                console.log(err);
            });
    };
}

// run the Charity front end 窗口加载执行初始化
window.onload = () => {
    initDomEvent();
};
