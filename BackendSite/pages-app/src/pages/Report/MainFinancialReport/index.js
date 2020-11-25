import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment, { isDate } from 'moment';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

const App = () => {
    const [stateFilter, setFilter] = useState({
        'SiteIds': [],
        'Currency': [],
        'StartDate': moment().add(-14, 'days').format('l'),
        'EndDate': moment().format('l')
    });

    const [stateDatas, setDatas] = useState({
        Datas: null,
        FilterInfo: null,
        CurrencyList: null,
        TotalResult: null
    });

    const [siteList, setSiteList] = useState();

    const [SubmitStatus, setSubmitStatus] = useState(false);
    const [ExportStatus, setExportStatus] = useState(false);

    useEffect(() => {
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }, []);

    const Filter_OnChange = (v, dataColumn) => {
        setFilter({
            ...stateFilter,
            [dataColumn]: v
        });
    }

    const Filter_CheckBox_OnChange = (v, dataColumn) => {
        let isRemove = stateFilter[dataColumn].includes(v);
        let newChecked = isRemove
            ? stateFilter[dataColumn].filter(value => value !== v)
            : stateFilter[dataColumn].push(v);
        let currentTab = stateFilter[dataColumn];
        if (newChecked.length === 0) {
            currentTab = [];
        } else if (currentTab === null) {
            currentTab = newChecked;
        }

        if (isRemove && !newChecked.includes(currentTab)) {
            currentTab = newChecked.length === 0 ? [] : newChecked;
        }
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
        if (v !== "All") return false;
        let currentTab = (stateFilter[dataColumn].length === options.length)
            ? []
            : options.map(o => {
                return o.value
            })
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_Submit = () => {
        if (stateFilter.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }

        if (stateFilter.Currency.length === 0) {
            alert("Please Choose at least one currency")
            return false;
        }

        if (!moment(stateFilter.StartDate).unix()) {
            alert("Please Choose Start Date")
            return false;
        }

        if (!moment(stateFilter.EndDate).unix()) {
            alert("Please Choose End Date")
            return false;
        }

        const currencyIds=[];
        stateFilter.Currency.map(o => {
            currencyIds.push(o.value);
       });

       setSubmitStatus(true);

        let param = {
            Url: 'Report/GetMainFinancialReport',
            Data: qs.stringify({
                SiteIds: stateFilter.SiteIds,
                CurrencyIds: currencyIds,
                StartDate: moment(stateFilter.StartDate).unix(),
                EndDate: moment(stateFilter.EndDate).add(1, 'days').unix() - 1
            })
        };

        setDatas({
            Datas:null,
            FilterInfo:[],
            CurrencyList:[],
            TotalResult:[]
        });

        WebAPI.PostData(param).then((res) => {

            setSubmitStatus(false);
            
            if (res.data.errorCode === 0) {

                let result = [];
                let totalResult = [];
                let currencyList =[];

                stateFilter.Currency.map(curr=>{
                    currencyList.push
                    ({
                        id:curr.value,
                        code:curr.label
                    });
                });

                totalResult.push({
                    'date':"TOTAL：",

                    'depositSummary':0,
                    'withdrawalSummary':0,
                    'bonusSummary':0,
                    'rebateSummary':0,
                    'adjustmentSummary':0,
                    'turnOverStakeSummary':0,
                    'turnOverValidBetSummary':0,
                    'winLossSummary':0                          
                });

                let totalITem=totalResult[0];

                currencyList.map(curr=>{

                    totalITem[`deposit_${curr.id}`] = 0;
                    totalITem[`withdrawal_${curr.id}`] = 0;
                    totalITem[`bonus_${curr.id}`] = 0;
                    totalITem[`rebate_${curr.id}`] = 0;
                    totalITem[`adjustment_${curr.id}`] = 0;
                    totalITem[`turnOverStake_${curr.id}`] = 0;
                    totalITem[`turnOverValidBet_${curr.id}`] = 0;
                    totalITem[`winLoss_${curr.id}`] = 0;

                });

                if(res.data.data.length !== 0){

                    res.data.data.forEach(elem => {

                        let getItem = result.find(x=> x.date === elem.date);
                        let depositColDef = `deposit_${elem.currencyId}`;
                        let withdrawalColDef = `withdrawal_${elem.currencyId}`;
                        let bonusColDef = `bonus_${elem.currencyId}`;
                        let rebateColDef = `rebate_${elem.currencyId}`;
                        let adjustmentColDef = `adjustment_${elem.currencyId}`;
                        let turnOverStakeColDef = `turnOverStake_${elem.currencyId}`;
                        let turnOverValidBetColDef = `turnOverValidBet_${elem.currencyId}`;
                        let winLossColDef = `winLoss_${elem.currencyId}`;

                        if(getItem){
                            getItem.depositSummary += elem.deposit;
                            getItem.withdrawalSummary += elem.withdrawal;
                            getItem.bonusSummary += elem.bonus;
                            getItem.rebateSummary += elem.rebate;
                            getItem.adjustmentSummary += elem.adjustment;
                            getItem.turnOverStakeSummary += elem.turnOverStake;
                            getItem.turnOverValidBetSummary += elem.turnOverValidBet;
                            getItem.winLossSummary += elem.winLoss;

                            getItem[depositColDef] = elem.deposit;
                            getItem[withdrawalColDef] = elem.withdrawal;
                            getItem[bonusColDef] = elem.bonus;
                            getItem[rebateColDef] = elem.rebate;
                            getItem[adjustmentColDef] = elem.adjustment;
                            getItem[turnOverStakeColDef] = elem.turnOverStake;
                            getItem[turnOverValidBetColDef] = elem.turnOverValidBet;
                            getItem[winLossColDef] = elem.winLoss;
                        }
                        else
                        {
                            result.push({
                                'date':elem.date,

                                'depositSummary':elem.deposit,
                                'withdrawalSummary':elem.withdrawal,
                                'bonusSummary':elem.bonus,
                                'rebateSummary':elem.rebate,
                                'adjustmentSummary':elem.adjustment,
                                'turnOverStakeSummary':elem.turnOverStake,
                                'turnOverValidBetSummary':elem.turnOverValidBet,
                                'winLossSummary':elem.winLoss,

                                [depositColDef]:elem.deposit,
                                [withdrawalColDef]:elem.withdrawal,
                                [bonusColDef]:elem.bonus,
                                [rebateColDef]:elem.rebate,
                                [adjustmentColDef]:elem.adjustment,
                                [turnOverStakeColDef]:elem.turnOverStake,
                                [turnOverValidBetColDef]:elem.turnOverValidBet,
                                [winLossColDef]:elem.winLoss                            
                            });
                        }

                        totalITem.depositSummary = totalITem.depositSummary + elem.deposit;
                        totalITem.withdrawalSummary = totalITem.withdrawalSummary + elem.withdrawal;
                        totalITem.bonusSummary = totalITem.bonusSummary + elem.bonus;
                        totalITem.rebateSummary = totalITem.rebateSummary + elem.rebate;
                        totalITem.adjustmentSummary = totalITem.adjustmentSummary + elem.adjustment;
                        totalITem.turnOverStakeSummary = totalITem.turnOverStakeSummary + elem.turnOverStake;
                        totalITem.turnOverValidBetSummary = totalITem.turnOverValidBetSummary + elem.turnOverValidBet;
                        totalITem.winLossSummary = totalITem.winLossSummary + elem.winLoss;

                        totalITem[depositColDef] = totalITem[depositColDef] + elem.deposit;
                        totalITem[withdrawalColDef] = totalITem[withdrawalColDef] + elem.withdrawal;
                        totalITem[bonusColDef] = totalITem[bonusColDef] + elem.bonus;
                        totalITem[rebateColDef] = totalITem[rebateColDef] + elem.rebate;
                        totalITem[adjustmentColDef] = totalITem[adjustmentColDef] + elem.adjustment;
                        totalITem[turnOverStakeColDef] = totalITem[turnOverStakeColDef] + elem.turnOverStake;
                        totalITem[turnOverValidBetColDef] = totalITem[turnOverValidBetColDef] + elem.turnOverValidBet;
                        totalITem[winLossColDef] = totalITem[winLossColDef] + elem.winLoss;

                    });
                }

                setDatas(
                    {
                        Datas: [
                            ...result.map(item=>{
                                    Object.keys(item).forEach(key => {
                                        if(key.startsWith('deposit') || 
                                           key.startsWith('withdrawal') ||
                                           key.startsWith('bonus') || 
                                           key.startsWith('rebate') ||
                                           key.startsWith('adjustment') || 
                                           key.startsWith('turnOverStake') ||
                                           key.startsWith('turnOverValidBet') || 
                                           key.startsWith('winLoss')){
                                            item[key] = number_format(number_toFixed(item[key], 2));
                                        }
                                    });
        
                                    return {...item}
                                })
                        ],
                        FilterInfo: {
                            ...stateFilter
                        },
                        CurrencyList: [...currencyList],
                        TotalResult: [
                            ...totalResult.map(item=>{
                            Object.keys(item).forEach(key => {
                                if(key.startsWith('deposit') || 
                                   key.startsWith('withdrawal') ||
                                   key.startsWith('bonus') || 
                                   key.startsWith('rebate') ||
                                   key.startsWith('adjustment') || 
                                   key.startsWith('turnOverStake') ||
                                   key.startsWith('turnOverValidBet') || 
                                   key.startsWith('winLoss'))
                                   {
                                      item[key] = number_format(number_toFixed(item[key], 2));
                                   }
                                });
    
                                return {...item}
                            })
                        ]
                    });
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    //single currency export
    const SingleCurrencyResult_ExportToExcel = () => {
        if (stateDatas.FilterInfo.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        if (stateDatas.FilterInfo.Currency.length === 0) {
            alert("Please Choose at least one Currency")
            return false;
        }

        const currencyIds=[];
        stateFilter.Currency.map(o => {
            currencyIds.push(o.value);
       });
        let param = {
            Url: 'Report/ExportMainFinancialReport',
            Data: {
                SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
                CurrencyIds: JSON.stringify(currencyIds),
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1
            }
        };

        WebAPI.DownloadData(param);
    }

    const ExportToExcel = () => {
        if (stateDatas.FilterInfo.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        if (stateDatas.FilterInfo.Currency.length === 0) {
            alert("Please Choose at least one Currency")
            return false;
        }
        if (!moment(stateDatas.FilterInfo.StartDate).unix()) {
            alert("Please Choose Start Date")
            return false;
        }

        if (!moment(stateDatas.FilterInfo.EndDate).unix()) {
            alert("Please Choose End Date")
            return false;
        }

        const currencyIds=[];
        stateFilter.Currency.map(o => {
            currencyIds.push(o.value);
       });
       
       setSubmitStatus(true);
       setExportStatus(true);

        let param = {
            Url: 'Report/ExportMainFinancialReport',
            Data: {
                SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
                CurrencyIds: JSON.stringify(currencyIds),
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1
            }
        };

        WebAPI.DownloadData(param);

        setTimeout(() => {
            setSubmitStatus(false);
            setExportStatus(false);
        }, 5000);
    }

        return (
            <div className="container-fluid" style={{ width: "100%", height: "100%", padding: "15px" }}>
                <h4 className="page-name">Main Financial Report</h4>
                <Filter filterInfo={stateFilter} 
                        onChange={Filter_OnChange} 
                        onChangeCheckBox={Filter_CheckBox_OnChange} 
                        onChangeAllCheckBox={Filter_CheckBox_CheckAll} 
                        ClickSubmit={Filter_Submit} 
                        SiteList={siteList}
                        SubmitBtnDisabled={SubmitStatus} />
                {stateDatas.Datas === null ? null : <Result {...stateDatas} ExportToExcel={ExportToExcel} ExportBtnDisabled={ExportStatus}/>}
            </div>
        );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}