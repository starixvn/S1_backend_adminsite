import React, {useState, useEffect} from "react";
import ReactDOM from 'react-dom';
import {OuterPanel}  from './components';
import {PayInfoHeader, PayInfoContent} from './PayInfo';
import WebAPI from '../../../module/WebAPI';
import {Type, Mode, AmountRule, Status} from './components'
import qs from 'qs';

if(process.env.NODE_ENV!=='production'){
    window.SiteList = {1:"Nova88", 2:"Bwing", 3:"123Goal"};
}

const App = ()=>{
    return(
        <div className="container-fluid">
            <h4 className="page-name">Payment Management</h4>
            <PayInfoList />
        </div>
    )
}

const level = {
    1: 'channelList',
    2: 'currencyList'
}

const PayInfoList = (props)=>{
    const [stateDatas, updateDatas] = useState(
    //     [
    //     {
    //         payName:'Help2Pay', 
    //         status:1, 
    //         channelList:[
    //             {
    //                 channelName:'Help2Pay',
    //                 type:'Online Banking',
    //                 mode:'開新視窗，將html內容置入',
    //                 sort:1,
    //                 status:1,
    //                 isEdit:false,
    //                 editValue:{

    //                 },
    //                 currencyList:[
    //                     {
    //                         currencyId:2,
    //                         currencyName:'MYR',
    //                         minAmount:1,
    //                         maxAmount:50,
    //                         amountRule:1,
    //                         isEdit:false
    //                     },
    //                     {
    //                         currencyId:4,
    //                         currencyName:'THB',
    //                         minAmount:50,
    //                         maxAmount:500,
    //                         amountRule:1,
    //                         isEdit:false
    //                     },
    //                     {
    //                         currencyId:15,
    //                         currencyName:'IDR',
    //                         minAmount:20000,
    //                         maxAmount:200000,
    //                         amountRule:1,
    //                         isEdit:false
    //                     }
    //                 ]
    //             }
    //         ]
    //     }
    //  ]
    );

    useEffect(() => {
        const fetchData = async () => {
            let param = {
                Url: 'PaymentManage/GetPayInfoList'
            };
            
            await WebAPI.PostData(param).then((response) => {
                if (response.data.errorCode === 0) {
                    let payList = response.data.data.payList;
                    let currencyList = response.data.data.currencyList;
                    let channelList = response.data.data.channelList;
                    let result = [];

                    payList.forEach(payInfo => {
                        let channels = channelList.filter(channel=>{ return channel.payId === payInfo.id}).map(channel => {

                            let currencies = currencyList.filter(currency=>{ return currency.channelId===channel.id}).map(currency=>{
                                return {
                                    ...currency,
                                    amountRule:{value:currency.amountRule, displayText:AmountRule[currency.amountRule]},
                                    isEdit:false
                                }
                            });

                            return {
                                ...channel,
                                isEdit:false,
                                type:{value:channel.type, displayText:Type[channel.type]},
                                mode:{value:channel.mode, displayText:Mode[channel.mode]},
                                currencyList:[...currencies],
                                status:{value:channel.status, displayText:Status[channel.status]}
                            }
                        });


                        result.push({
                            ...payInfo,
                            channelList:channels
                        });
                    });

                    updateDatas(result)
                }else{
                    alert(response.data.message)
                }
            });
        };

        fetchData();
    }, []);

    const UpdatePayInfoStatus = (idx)=>{
        let status = stateDatas[idx].status === 1 ? 2 : 1;
        if(window.confirm(`${status===1 ? "Open?" : "Disable"} ${stateDatas[idx].payName}?`)){
            stateDatas[idx].status = status;
            updateDatas([...stateDatas]);
        }
    }

    const ClickEdit = (idxArr)=>{
        let elem = stateDatas;
        idxArr.forEach((ArrIdx, idx) => {
            elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
        });

        elem.isEdit = true;
        
        if(idxArr.length===2){
            let {currencyList, isEdit, editValue, ...newValue} = elem;
            elem['editValue'] = {...newValue};
        }else if(idxArr.length===3){
            let {isEdit, ...newValue} = elem;
            elem['editValue'] = {...newValue};
        }
        
        updateDatas([...stateDatas]);
    }

    const ClickCancel = (idxArr)=>{
        if(window.confirm('cancel modify?')){
            let elem = stateDatas;
            idxArr.forEach((ArrIdx, idx) => {
                elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
            });

            elem.isEdit = false;
            elem['editValue'] = {};
            updateDatas([...stateDatas]);
        }
    }

    const EditField = (value, dataColumn, idxArr)=>{
        let elem = stateDatas;
        idxArr.forEach((ArrIdx, idx) => {
            elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
        });

        if(typeof(elem.editValue[dataColumn])==="object"){
            elem.editValue[dataColumn].value = value;
        }else{
            elem.editValue[dataColumn] = value;
        }

        updateDatas([...stateDatas]);
    }

    const ClickSave = (idxArr)=>{
        if(idxArr.length===3){
            let elem = stateDatas[idxArr[0]].channelList[idxArr[1]].currencyList[idxArr[2]];
            stateDatas[idxArr[0]][level[1]][idxArr[1]][level[2]][idxArr[2]] = {
                ...elem,
                ...elem.editValue,
                isEdit:false,
                editValue:{}
            }
        }else if(idxArr.length===2){
            let elem = stateDatas[idxArr[0]].channelList[idxArr[1]];
            stateDatas[idxArr[0]].channelList[idxArr[1]] = {
                ...elem,
                ...elem.editValue,
                isEdit:false,
                editValue:{}
            }
        }

        updateDatas([...stateDatas]);
    }

    let payList = stateDatas ? stateDatas.map((item, idx)=>{
        let id =`PayInfo-${idx}`;
        return(
            <OuterPanel
                key={`payInfoList-${idx}`}
                Id={id}
                HeaderContent={
                    <PayInfoHeader Name={item.payName}/>
                }
                Content={
                    <PayInfoContent
                        idxArr={[idx]}
                        MainId={id}
                        switchBtn={{
                            Status:item.status,
                            onChange:UpdatePayInfoStatus.bind(null, idx),
                            SwitchBtnKey:`payInfo-status-switch-${idx}`
                        }}
                        ChannelList={item.channelList}
                        ClickEdit={ClickEdit}
                        ClickCancel={ClickCancel}
                        ClickSave={ClickSave}
                        EditField={EditField}
                    />
                }
            />
        );
    }) : <div></div>;

    return payList
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}