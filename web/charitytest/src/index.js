import ReactDOM from 'react-dom';

import React, {Component} from 'react';
import {Input, Button} from "antd";
import 'antd/dist/antd.css';
import AElf from 'aelf-sdk';


const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));
let newWallet = AElf.wallet.createNewWallet();
const contractName = 'AElf.ContractNames.HelloWorldContract';
let contractAddress;

let charityTestContract;

(async () => {
    const chainStatus = await aelf.chain.getChainStatus();
    const GenesisContractAddress = chainStatus.GenesisContractAddress;
    const zeroContract = await aelf.chain.contractAt(GenesisContractAddress, newWallet);
    contractAddress = await zeroContract.GetContractAddressByName.call(AElf.utils.sha256(contractName));
})();




class App extends Component {

    async  F1(){

        let a = document.getElementById("a").innerText;
        let b = document.getElementById("b").innerText;

        charityTestContract = await aelf.chain.contractAt(contractAddress, newWallet);
        (async() =>{
            const result = await charityTestContract.Hello.call();
            if(!aelf.isConnected()) {
                alert('Blockchain Node is not running.');
            }
            else {
                document.getElementById("b").setAttribute('Value', result.value); // value小写
            }
        })();


/*
        //console.log("123");
        aelf.chain.contractAt(contractAddress, newWallet)
            .then(result => {
                charityTestContract = result;
            });
*/


    }




    render(){
        return(

            <div>
                <Input id = "a"/>
                <Input id = "b"/>
                <Button onClick={()=>this.F1()}>提交!</Button>
                <Input id = "result"/>result
            </div>

        );
    }
}



ReactDOM.render(
    <App />,
    document.getElementById('root')
);





/*
import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as serviceWorker from './serviceWorker';

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
*/
