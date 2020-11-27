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

    const [HiddenTitles, setHiddenTitles] = useState({hiddenTitlesList:[]});

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

    const Result_HideShowBtnOnClick = (element) => {

        let list = [];
        let getItem = HiddenTitles.hiddenTitlesList.find(x=> x.Category === element.columnTitle);

        if(getItem)
        {
            HiddenTitles.hiddenTitlesList.forEach(elem=>{
                if(elem.Category!==getItem.Category)
                {
                    list.push(elem);
                }
            });
        }
        else
        {
            list=HiddenTitles.hiddenTitlesList;
            list.push({
                'Category':element.columnTitle,
                'Group':element.columnTitle,
                'Header':element.headerStartwith
            });
        }

        setHiddenTitles({
            hiddenTitlesList:list
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
                // StartDate: stateFilter.StartDate,
                // EndDate: stateFilter.EndDate,
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
                    'nettSummary':0,
                    'bonusSummary':0,
                    'rebateSummary':0,
                    'adjustmentSummary':0,
                    'turnOverStakeSummary':0,
                    'turnOverValidBetSummary':0,
                    'winLossSummary':0                          
                });

                let totalItem=totalResult[0];

                currencyList.map(curr=>{

                    totalItem[`deposit_${curr.id}`] = 0;
                    totalItem[`withdrawal_${curr.id}`] = 0;
                    totalItem[`nett_${curr.id}`] = 0;
                    totalItem[`bonus_${curr.id}`] = 0;
                    totalItem[`rebate_${curr.id}`] = 0;
                    totalItem[`adjustment_${curr.id}`] = 0;
                    totalItem[`turnOverStake_${curr.id}`] = 0;
                    totalItem[`turnOverValidBet_${curr.id}`] = 0;
                    totalItem[`winLoss_${curr.id}`] = 0;

                });

                if(res.data.data.length !== 0){

                    res.data.data.forEach(elem => {

                        let getItem = result.find(x=> x.date === elem.date);
                        let depositColDef = `deposit_${elem.currencyId}`;
                        let withdrawalColDef = `withdrawal_${elem.currencyId}`;
                        let nettColDef = `nett_${elem.currencyId}`;
                        let bonusColDef = `bonus_${elem.currencyId}`;
                        let rebateColDef = `rebate_${elem.currencyId}`;
                        let adjustmentColDef = `adjustment_${elem.currencyId}`;
                        let turnOverStakeColDef = `turnOverStake_${elem.currencyId}`;
                        let turnOverValidBetColDef = `turnOverValidBet_${elem.currencyId}`;
                        let winLossColDef = `winLoss_${elem.currencyId}`;

                        if(getItem){
                            getItem.depositSummary += elem.deposit * elem.actualRate;
                            getItem.withdrawalSummary += elem.withdrawal * elem.actualRate;
                            getItem.nettSummary += elem.nett * elem.actualRate;
                            getItem.bonusSummary += elem.bonus * elem.actualRate;
                            getItem.rebateSummary += elem.rebate * elem.actualRate;
                            getItem.adjustmentSummary += elem.adjustment * elem.actualRate;
                            getItem.turnOverStakeSummary += elem.turnOverStake * elem.actualRate;
                            getItem.turnOverValidBetSummary += elem.turnOverValidBet * elem.actualRate;
                            getItem.winLossSummary += elem.winLoss * elem.actualRate;

                            getItem[depositColDef] = elem.deposit;
                            getItem[withdrawalColDef] = elem.withdrawal;
                            getItem[nettColDef] = elem.nett;
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

                                'depositSummary':elem.deposit * elem.actualRate,
                                'withdrawalSummary':elem.withdrawal * elem.actualRate,
                                'nettSummary':elem.nett * elem.actualRate,
                                'bonusSummary':elem.bonus * elem.actualRate,
                                'rebateSummary':elem.rebate * elem.actualRate,
                                'adjustmentSummary':elem.adjustment * elem.actualRate,
                                'turnOverStakeSummary':elem.turnOverStake * elem.actualRate,
                                'turnOverValidBetSummary':elem.turnOverValidBet * elem.actualRate,
                                'winLossSummary':elem.winLoss * elem.actualRate,

                                [depositColDef]:elem.deposit,
                                [withdrawalColDef]:elem.withdrawal,
                                [nettColDef]:elem.nett,
                                [bonusColDef]:elem.bonus,
                                [rebateColDef]:elem.rebate,
                                [adjustmentColDef]:elem.adjustment,
                                [turnOverStakeColDef]:elem.turnOverStake,
                                [turnOverValidBetColDef]:elem.turnOverValidBet,
                                [winLossColDef]:elem.winLoss                            
                            });
                        }

                        totalItem.depositSummary = totalItem.depositSummary + elem.deposit * elem.actualRate;
                        totalItem.withdrawalSummary = totalItem.withdrawalSummary + elem.withdrawal * elem.actualRate;
                        totalItem.nettSummary = totalItem.nettSummary + elem.nett * elem.actualRate;
                        totalItem.bonusSummary = totalItem.bonusSummary + elem.bonus * elem.actualRate;
                        totalItem.rebateSummary = totalItem.rebateSummary + elem.rebate * elem.actualRate;
                        totalItem.adjustmentSummary = totalItem.adjustmentSummary + elem.adjustment * elem.actualRate;
                        totalItem.turnOverStakeSummary = totalItem.turnOverStakeSummary + elem.turnOverStake * elem.actualRate;
                        totalItem.turnOverValidBetSummary = totalItem.turnOverValidBetSummary + elem.turnOverValidBet * elem.actualRate;
                        totalItem.winLossSummary = totalItem.winLossSummary +elem.winLoss * elem.actualRate;

                        totalItem[depositColDef] = totalItem[depositColDef] + elem.deposit;
                        totalItem[withdrawalColDef] = totalItem[withdrawalColDef] + elem.withdrawal;
                        totalItem[nettColDef] = totalItem[nettColDef] + elem.nett;
                        totalItem[bonusColDef] = totalItem[bonusColDef] + elem.bonus;
                        totalItem[rebateColDef] = totalItem[rebateColDef] + elem.rebate;
                        totalItem[adjustmentColDef] = totalItem[adjustmentColDef] + elem.adjustment;
                        totalItem[turnOverStakeColDef] = totalItem[turnOverStakeColDef] + elem.turnOverStake;
                        totalItem[turnOverValidBetColDef] = totalItem[turnOverValidBetColDef] + elem.turnOverValidBet;
                        totalItem[winLossColDef] = totalItem[winLossColDef] + elem.winLoss;

                    });
                }

                setDatas(
                    {
                        Datas: [
                            ...result.map(item=>{
                                    Object.keys(item).forEach(key => {
                                        if(key.startsWith('deposit') || 
                                           key.startsWith('withdrawal') ||
                                           key.startsWith('nett') ||
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
                                   key.startsWith('nett') ||
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
                // StartDate: stateDatas.FilterInfo.StartDate,
                // EndDate: stateDatas.FilterInfo.EndDate
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
                {stateDatas.Datas === null 
                ? null 
                : <Result {...stateDatas} 
                   ExportToExcel={ExportToExcel} 
                   ExportBtnDisabled={ExportStatus}
                   HidenTitles={HiddenTitles}
                   HideShowBtn={Result_HideShowBtnOnClick}/>}
            </div>
        );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}