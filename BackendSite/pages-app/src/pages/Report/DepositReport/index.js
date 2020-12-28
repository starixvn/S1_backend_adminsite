import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';
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

        let param = {
            Url: 'Report/GetDepositReport',
            Data: qs.stringify({
                SiteIds: stateFilter.SiteIds,
                CurrencyIds: currencyIds,
                StartDate: moment(stateFilter.StartDate).format('YYYY-MM-DD'),
                EndDate: moment(stateFilter.EndDate).format('YYYY-MM-DD')
            })
        };

        setSubmitStatus(true);
        setExportStatus(true);

        WebAPI.PostData(param).then((res) => {

            setSubmitStatus(false);
            setExportStatus(false);


            if (res.data.errorCode === 0) {
                let result = [];
                let currencyList =[];
                let totalResult=[];

                stateFilter.Currency.map(curr=>{
                    currencyList.push
                    ({
                        id:curr.value,
                        code:curr.label
                    });
                });

                totalResult.push({
                    'accountName':"TOTAL：",
    
                    'onlineSummary':res.data.data[0].summaryTotalData.onlineDeposit,
                    'onlinePrecentSummary':res.data.data[0].summaryTotalData.onlinePrecent,
                    'mobileSummary':res.data.data[0].summaryTotalData.mobileDeposit,
                    'mobilePrecentSummary':res.data.data[0].summaryTotalData.mobilePrecent,
                    'localSummary':res.data.data[0].summaryTotalData.localDeposit,
                    'localPrecentSummary':res.data.data[0].summaryTotalData.localPrecent,

                    'totalAmountSummary':res.data.data[0].summaryTotalData.totalAmount,
                    'totalCountSummary':res.data.data[0].summaryTotalData.totalCount,
                    'averangeAmountSummary':res.data.data[0].summaryTotalData.averangeDeposit,
                    'successRateSummary':res.data.data[0].summaryTotalData.successRate,
                    'ftdCountSummary':res.data.data[0].summaryTotalData.ftdCount,
                    'apTimeSummary':res.data.data[0].summaryTotalData.averangeProcessingTime    
                });
    
                let totalItem=totalResult[0];
    
                currencyList.map(curr=>{
    
                    totalItem[`onlineSummary_${curr.id}`] = 0;
                    totalItem[`onlinePrecent_${curr.id}`] = 0;
                    totalItem[`mobileSummary_${curr.id}`] = 0;
                    totalItem[`mobilePrecent_${curr.id}`] = 0;
                    totalItem[`localSummary_${curr.id}`] = 0;
                    totalItem[`localPrecent_${curr.id}`] = 0;

                    totalItem[`totalAmount_${curr.id}`] = 0;
                    totalItem[`totalCount_${curr.id}`] = 0;
                    totalItem[`averangeAmount_${curr.id}`] = 0;
                    totalItem[`successRate_${curr.id}`] = 0;
                    totalItem[`ftdCount_${curr.id}`] = 0;
                    totalItem[`apTime_${curr.id}`] = 0;
                });

                res.data.data[0].summaryAccountData.forEach(elem => {

                    result.push({

                        'accountType':elem.accountType,
                        'accountId':elem.accountId,
                        'accountName':elem.accountName,

                        'onlineSummary': elem.summaryTotalData.onlineDeposit,
                        'onlinePrecentSummary': elem.summaryTotalData.onlinePrecent,
                        'mobileSummary': elem.summaryTotalData.mobileDeposit,
                        'mobilePrecentSummary': elem.summaryTotalData.mobilePrecent,
                        'localSummary': elem.summaryTotalData.localDeposit,
                        'localPrecentSummary': elem.summaryTotalData.localPrecent,

                        'totalAmountSummary': elem.summaryTotalData.totalAmount,
                        'totalCountSummary': elem.summaryTotalData.totalCount,
                        'averangeAmountSummary': elem.summaryTotalData.averangeDeposit,
                        'successRateSummary': elem.summaryTotalData.successRate,
                        'ftdCountSummary': elem.summaryTotalData.ftdCount,
                        'apTimeSummary': elem.summaryTotalData.averangeProcessingTime
                    });

                    let getItem = result.find(x=>x.accountType === elem.accountType 
                                              && x.accountId === elem.accountId);

                    elem.summaryData.forEach(data => {
                            
                        let onlineColDef = `onlineSummary_${data.currencyId}`;
                        let onlinePrecentColDef = `onlinePrecent_${data.currencyId}`;
                        let mobileColDef = `mobileSummary_${data.currencyId}`;
                        let mobilePrecentColDef = `mobilePrecent_${data.currencyId}`;
                        let localColDef = `localSummary_${data.currencyId}`;
                        let localPrecentColDef = `localPrecent_${data.currencyId}`;
    
                        let totalAmountColDef = `totalAmount_${data.currencyId}`;
                        let totalCountColDef = `totalCount_${data.currencyId}`;
                        let averangeAmountColDef = `averangeAmount_${data.currencyId}`;
                        let successRateColDef = `successRate_${data.currencyId}`;
                        let ftdCountColDef = `ftdCount_${data.currencyId}`;
                        let apTimeColDef = `apTime_${data.currencyId}`;

                        //#region Detail Data

                        getItem[onlineColDef] = data.onlineDeposit;
                        getItem[onlinePrecentColDef] = data.onlinePrecent;
                        getItem[mobileColDef] = data.mobileDeposit;
                        getItem[mobilePrecentColDef] = data.mobilePrecent;
                        getItem[localColDef] = data.localDeposit;
                        getItem[localPrecentColDef] = data.localPrecent;

                        getItem[totalAmountColDef] = data.totalAmount;
                        getItem[totalCountColDef] = data.totalCount;
                        getItem[averangeAmountColDef] = data.averangeDeposit;
                        getItem[successRateColDef] = data.successRate;
                        getItem[ftdCountColDef] = data.ftdCount;
                        getItem[apTimeColDef] = data.averangeProcessingTime;

                        //#endregion

                    });

                });

                res.data.data[0].summaryCurrencyData.forEach(elem => {

                        let onlineColDef = `onlineSummary_${elem.currencyId}`;
                        let onlinePrecentColDef = `onlinePrecent_${elem.currencyId}`;
                        let mobileColDef = `mobileSummary_${elem.currencyId}`;
                        let mobilePrecentColDef = `mobilePrecent_${elem.currencyId}`;
                        let localColDef = `localSummary_${elem.currencyId}`;
                        let localPrecentColDef = `localPrecent_${elem.currencyId}`;
    
                        let totalAmountColDef = `totalAmount_${elem.currencyId}`;
                        let totalCountColDef = `totalCount_${elem.currencyId}`;
                        let averangeAmountColDef = `averangeAmount_${elem.currencyId}`;
                        let successRateColDef = `successRate_${elem.currencyId}`;
                        let ftdCountColDef = `ftdCount_${elem.currencyId}`;
                        let apTimeColDef = `apTime_${elem.currencyId}`;

                        //#region Detail Data

                        totalItem[onlineColDef] = elem.onlineDeposit;
                        totalItem[onlinePrecentColDef] = elem.onlinePrecent;
                        totalItem[mobileColDef] = elem.mobileDeposit;
                        totalItem[mobilePrecentColDef] = elem.mobilePrecent;
                        totalItem[localColDef] = elem.localDeposit;
                        totalItem[localPrecentColDef] = elem.localPrecent;

                        totalItem[totalAmountColDef] = elem.totalAmount;
                        totalItem[totalCountColDef] = elem.totalCount;
                        totalItem[averangeAmountColDef] = elem.averangeDeposit;
                        totalItem[successRateColDef] = elem.successRate;
                        totalItem[ftdCountColDef] = elem.ftdCount;
                        totalItem[apTimeColDef] = elem.averangeProcessingTime;

                        //#endregion

                });

                setDatas(
                    {
                        Datas:[
                            ...result.map(item=>{
                                    Object.keys(item).forEach(key => {
                                        if(key.startsWith('onlineSummary') || 
                                           key.startsWith('mobileSummary') ||
                                           key.startsWith('localSummary') ||
                                           key.startsWith('totalAmount') || 
                                           key.startsWith('averangeAmount')){
                                            item[key] = number_format(number_toFixed(item[key], 2));
                                        }
                                    });
        
                                    return {...item}
                                })
                        ],
                        FilterInfo: {
                            ...stateFilter
                        },
                        CurrencyList: currencyList,
                        TotalResult:[
                            ...totalResult.map(item=>{
                                    Object.keys(item).forEach(key => {
                                        if(key.startsWith('onlineSummary') || 
                                           key.startsWith('mobileSummary') ||
                                           key.startsWith('localSummary') ||
                                           key.startsWith('totalAmount') || 
                                           key.startsWith('averangeAmount')){
                                            item[key] = number_format(number_toFixed(item[key], 2));
                                        }
                                    });
        
                                    return {...item}
                                })
                        ]
                    }
                )
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const ExportToExcel = () => {
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
       setExportStatus(true);

        let param = {
            Url: 'Report/ExportDepositReport',
            Data: {
                SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
                CurrencyIds: JSON.stringify(currencyIds),
                StartDate: moment(stateDatas.FilterInfo.StartDate).format('YYYY-MM-DD'),
                EndDate: moment(stateDatas.FilterInfo.EndDate).format('YYYY-MM-DD')
            }
        };

        WebAPI.DownloadData(param);

        setTimeout(() => {
            setSubmitStatus(false);
            setExportStatus(false);
        }, 5000);
    }

    return (
        <div className="container-fluid" style={{width:"auto"}}>
            <h4 className="page-name">Deposit Report</h4>
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
                      ExportBtnDisabled={ExportStatus}/>}
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}