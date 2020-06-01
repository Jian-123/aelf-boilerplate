import React from 'react';
import logo from './logo.svg';
import './App.css';
import { Button } from 'antd';

import {  Input,InputNumber  }from 'antd';

import AElf from 'aelf-sdk';

const { sha256 } = AElf.utils;

const newWallet = AElf.wallet.createNewWallet();

const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));




/*
//没用上
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
*/







const charityTest = 'AElf.ContractNames.CharityTest';
let charityTestContract;
let charityTestAddress;

(async () => {
    // 链状态
    const chainStatus = await aelf.chain.getChainStatus();
    // 创世合约地址
    if (!chainStatus) {
        throw new Error('Error occurred when getting chain status');
    }
    const GenesisContractAddress = chainStatus.GenesisContractAddress;
    // get genesis contract instance
    const zeroContract = await aelf.chain.contractAt(GenesisContractAddress, newWallet);
    // 创世合约的只读方法获取系统合约地址

    charityTest


    async function  Test1() {

        const result = document.getElementById('result');
        charityTestContract = await aelf.chain.contractAt(charityTestAddress, newWallet);
        (async () => {

            let result1 = charityTestContract.TestAdd.call({a:1,b:2});
            console.log(result1.Value);

            return <div>result1</div>;


        })();


    }













    /*

    async function ClickButton() =

        // document.getElementById('result').setAttribute('Value', '1');
        Test1();
    */



    function App() {

        if(!aelf.isConnected()) {
            alert('BlockChain Node is not running.');
        }
        return (
            <div className="App">

                <Button type="primary" onClick = {Test1 }>Button</Button>

                <InputNumber>数字a </InputNumber>
                <InputNumber>数字b </InputNumber>
                <Input id = 'result'/>
            </div>
        );
    }

    export default App;
    Address = await zeroContract.GetContractAddressByName.call(AElf.utils.sha256(charityTest));;


})();