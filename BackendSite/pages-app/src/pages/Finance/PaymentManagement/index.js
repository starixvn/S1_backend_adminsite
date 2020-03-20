import React, {useState, useEffect} from "react";
import ReactDOM from 'react-dom';
import {OuterPanel, PanelClass}  from './components';
import {PayInfoHeader, PayInfoContent} from './PayInfo';
import WebAPI from '../../../module/WebAPI';
import {BankInfoContent} from './BankInfo';
import qs from 'qs';

if(process.env.NODE_ENV!=='production'){
    window.subFuncKeys = ["f_pm_editAmountRule"];
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

const webAPIUrl = {
    'GetPayInfoList':'PaymentManage/GetPayInfoList',
    'UpdatePayList':'PaymentManage/UpdatePayList',
    'UpdatePayChannel':'PaymentManage/UpdatePayChannel',
    'UpdatePayCurrency':'PaymentManage/UpdatePayCurrency',
    'UpdatePayCurrencyAmount':'PaymentManage/UpdatePayCurrencyAmount',
    'GetBankSysInfoList':'PaymentManage/GetBankSysInfoList',
    'UpdateBankInfo':'PaymentManage/UpdateBankInfo'
}

const PayInfoList = (props)=>{
    const [stateDatas, updateDatas] = useState();
    const [stateBankInfo, updateBankInfo] = useState({
        payName:'xxxx',
        bankList:[]
    });

    useEffect(() => {
        const fetchData = async () => {
            let param = {
                Url: webAPIUrl.GetPayInfoList
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
                                    amountShow:currency.amountShow ? currency.amountShow.replace('[','').replace(']','') : currency.amountShow,
                                    amountRule:currency.amountRule,
                                    isEdit:false
                                }
                            });

                            return {
                                ...channel,
                                isEdit:false,
                                type:channel.type,
                                mode:channel.mode,
                                currencyList:[...currencies],
                                status:channel.status
                            }
                        });


                        result.push({
                            ...payInfo,
                            channelList:channels
                        });
                    });

                    updateDatas(result)
                }else{
                    alert(response.data.message ?? 'failed to fetch data from server.');
                }
            });
        };

        fetchData();
    }, []);

    const UpdatePayInfoStatus = (idx)=>{
        let status = stateDatas[idx].status === 1 ? 2 : 1;
        stateDatas[idx].status = status;
        let param = {
            Url: webAPIUrl.UpdatePayList,
            Data: qs.stringify({
                PayId:stateDatas[idx].id,
                Status: status
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                updateDatas([...stateDatas]);      
            } else {
                alert(res.data.message ?? 'failed to fetch data from server.');
            }
        })
    }

    const HandleCollaps = (idxArr)=>{
        let elem = stateDatas;
        idxArr.forEach((ArrIdx, idx) => {
            elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
        });

        elem['isCollaps'] = elem['isCollaps'] === undefined ? true : !elem['isCollaps'];

        updateDatas([...stateDatas]);
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
        let elem = stateDatas;
        idxArr.forEach((ArrIdx, idx) => {
            elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
        });

        elem.isEdit = false;
        elem['editValue'] = null;
        updateDatas([...stateDatas]);
    }

    const EditField = (value, dataColumn, idxArr)=>{
        let elem = stateDatas;
        idxArr.forEach((ArrIdx, idx) => {
            elem = level[idx] ? elem[level[idx]][ArrIdx] : elem[ArrIdx];
        });

        if(dataColumn==='status' && !elem.editValue){
            
            let param = {
                Url: webAPIUrl.UpdatePayChannel,
                Data: qs.stringify({
                    ChannelId:elem.id,
                    Sort:elem.sort,
                    Status:value,
                    Mode:elem.mode
                })
            };
            
            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    elem[dataColumn] = value;
                    updateDatas([...stateDatas]);
                } else {
                    alert(res.data.message ?? 'failed to fetch data from server.');
                }
            });
        }else{
            elem.editValue[dataColumn] = value;
            updateDatas([...stateDatas]);
        }
    }

    const ClickSave = (idxArr)=>{
        if(idxArr.length===3){
            let elem = stateDatas[idxArr[0]].channelList[idxArr[1]].currencyList[idxArr[2]];
            let param = window.subFuncKeys && window.subFuncKeys.includes('f_pm_editAmountRule') ? {
                Url: webAPIUrl.UpdatePayCurrency,
                Data: qs.stringify({
                    ChannelId:elem.channelId,
                    CurrencyId:elem.currencyId,
                    MinAmount:elem.editValue.minAmount,
                    MaxAmount:elem.editValue.maxAmount,
                    AmountRule:elem.editValue.amountRule,
                    AmountShow:elem.editValue.amountShow ? JSON.stringify(elem.editValue.amountShow.split(',').map(v=>{return Number(v)})) : elem.editValue.amountShow
                })
            } : {
                Url: webAPIUrl.UpdatePayCurrencyAmount,
                Data: qs.stringify({
                    ChannelId:elem.channelId,
                    CurrencyId:elem.currencyId,
                    MinAmount:elem.editValue.minAmount,
                    MaxAmount:elem.editValue.maxAmount
                })
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    stateDatas[idxArr[0]][level[1]][idxArr[1]][level[2]][idxArr[2]] = {
                        ...elem,
                        ...elem.editValue,
                        isEdit:false,
                        editValue:null
                    };

                    updateDatas([...stateDatas]);
                } else {
                    alert(res.data.message ?? 'failed to fetch data from server.');
                }
            });

        }else if(idxArr.length===2){
            let elem = stateDatas[idxArr[0]].channelList[idxArr[1]];

            let param = {
                Url: webAPIUrl.UpdatePayChannel,
                Data: qs.stringify({
                    ChannelId:elem.id,
                    Sort:elem.editValue.sort,
                    Status:elem.editValue.status
                })
            };
            
            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    stateDatas[idxArr[0]].channelList[idxArr[1]] = {
                        ...elem,
                        ...elem.editValue,
                        isEdit:false,
                        editValue:null
                    }

                    updateDatas([...stateDatas]);
                } else {
                    alert(res.data.message ?? 'failed to fetch data from server.');
                }
            });
        }
    }

    const Result_ContentOnClick = (dataColumn, idxArr)=>{
        updateBankInfo(null);

        let param = {
            Url: webAPIUrl.GetBankSysInfoList,
            Data: qs.stringify({
                PayId:stateDatas[idxArr[0]].id
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                updateBankInfo({
                    payId:stateDatas[idxArr[0]].id,
                    payName:stateDatas[idxArr[0]].payName,
                    bankList:[...res.data.data]
                });
            } else {
                alert(res.data.message ?? 'failed to fetch data from server.');
            }
        })
    }

    const Modal_EditField = (value, dataColumn, idx)=>{
        let param = {
            Url: webAPIUrl.UpdateBankInfo,
            Data: qs.stringify({
                PayId:stateBankInfo.payId,
                SysId:stateBankInfo.bankList[idx].id,
                Status:value
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                stateBankInfo.bankList[idx][dataColumn] = value;
                updateBankInfo({...stateBankInfo});
            } else {
                alert(res.data.message ?? 'failed to fetch data from server.');
            }
        });
    }

    let payList = stateDatas ? stateDatas.map((item, idx)=>{
        let id =`PayInfo-${idx}`;
        return(
            <OuterPanel
                PanelClass={PanelClass.level1_mt20}
                HandleCollaps={HandleCollaps}
                idxArr={[idx]}
                isCollaps={item.isCollaps}
                key={`payInfoList-${idx}`}
                Id={id}
                HeaderContent={
                    <PayInfoHeader 
                        Name={item.payName} 
                        switchBtn={{
                            Status:item.status,
                            onChange:UpdatePayInfoStatus.bind(null, idx),
                            SwitchBtnKey:`payInfo-status-switch-${idx}`
                        }}
                    />
                }
                Content={
                    <PayInfoContent
                        idxArr={[idx]}
                        MainId={id}                        
                        ChannelList={item.channelList}
                        ClickEdit={ClickEdit}
                        ClickCancel={ClickCancel}
                        ClickSave={ClickSave}
                        EditField={EditField}
                        ContentOnClick={Result_ContentOnClick}
                        HandleCollaps={HandleCollaps}
                    />
                }
            />
        );
    }) : <div></div>;

    return (
        <>
            <Modal 
                {...stateBankInfo} 
                EditField={Modal_EditField}
            />
            {payList}
        </>
    );
}

const Modal = (props)=>{
    return(
        <div className="modal fade" id="ModalScrollable" role="dialog" aria-labelledby="exampleModalScrollableTitle" aria-hidden="true">
            <div className="modal-dialog modal-dialog-scrollable" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="exampleModalScrollableTitle">{props.payName} - Bank Info</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <BankInfoContent
                            BankList={props.bankList}
                            EditField={props.EditField}
                        />
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}