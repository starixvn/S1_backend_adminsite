import React, {useState, useRef, useEffect} from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import moment from 'moment';
import qs from 'qs';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

const App = ()=>{

    const [stateFilter, setFilter] = useState({
        'SiteIds': []
    });

    const [stateData, setData] = useState({
        FilterInfo: null,
        Datas: null,
        CategoryTotalResult: null,
        TotalResult: null,
        CurrencyList: null,
        CategoryGameList: null
    });

    const [siteList, setSiteList] = useState(); 
    const [SubmitStatus, setSubmitStatus] = useState(false);
    const [ExportStatus, setExportStatus] = useState(false);

    const [HiddenTitles, setHiddenTitles] = useState({
        turnOverValidBetHiddenCategoryList:[],
        winLossHiddenCategoryList:[],
        hiddenTitlesList:[]
    });

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

    const Filter_OnChange = (v, dataColumn)=>{
        setFilter({
            ...stateFilter,
            [dataColumn]:v
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

    const Result_ValidBetHideShowBtnOnClick = (element) => {

        let turnOverValidBetHiddenlist = [];
        let getItem = HiddenTitles.turnOverValidBetHiddenCategoryList.find(x=> x.CategoryId === element);

        if(getItem)
        {
            HiddenTitles.turnOverValidBetHiddenCategoryList.forEach(elem=>{
                if(elem.CategoryId!==getItem.CategoryId)
                {
                    turnOverValidBetHiddenlist.push(elem);
                }
            });
        }
        else
        {
            turnOverValidBetHiddenlist=HiddenTitles.turnOverValidBetHiddenCategoryList;
            turnOverValidBetHiddenlist.push({
                         'CategoryId':element
            });
        }

        setHiddenTitles({
            turnOverValidBetHiddenCategoryList: turnOverValidBetHiddenlist,
            winLossHiddenCategoryList: HiddenTitles.winLossHiddenCategoryList
        });

    }

    const Result_WinLossHideShowBtnOnClick = (element) => {

        let winLossHiddenlist = [];
        let getItem = HiddenTitles.winLossHiddenCategoryList.find(x=> x.CategoryId === element);

        if(getItem)
        {
            HiddenTitles.winLossHiddenCategoryList.forEach(elem=>{
                if(elem.CategoryId!==getItem.CategoryId)
                {
                    winLossHiddenlist.push(elem);
                }
            });
        }
        else
        {
            winLossHiddenlist=HiddenTitles.winLossHiddenCategoryList;
            winLossHiddenlist.push({
                         'CategoryId':element
            });
        }

        setHiddenTitles({
            turnOverValidBetHiddenCategoryList: HiddenTitles.turnOverValidBetHiddenCategoryList,
            winLossHiddenCategoryList: winLossHiddenlist
        });

    }

    const Filter_Submit = ()=>{

        if (stateFilter.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }

        setSubmitStatus(true);

        let param = {
            Url: 'Report/GetStatisticalReport',
            Data: qs.stringify({
                SiteIds: stateFilter.SiteIds,
                EndDate: moment().format('YYYY-MM-DD')
            })
        };

        setData({
            ...stateData,
            Datas: null,
            CategoryTotalResult: null,
            TotalResult: null,
            CurrencyList: null,
            CategoryGameList: null
        });

        WebAPI.PostData(param).then((res) => {

            setSubmitStatus(false);

            if (res.data.errorCode === 0) {

                let totalResult = [];
                let result = [];
                let categoryGameList = [];
                let currencyList = [];
                let categoryTotalResult=[];
                let categoryTotalItem=null;

                if(res.data.data.length!==0){

                    res.data.data.forEach(elem => {

                        //add CurrencyList
                        if(!currencyList.find(x=>x.id===elem.currencyId)){
                            currencyList.push({
                                id:elem.currencyId,
                                code:elem.isoCode
                            });
                        }

                    });

                    totalResult.push({
                        'categoryName':" ",
                        'gameName':"TOTAL：",
    
                        'turnOverValidBetTodaySummary':0,
                        'turnOverValidBetYesterdaySummary':0,
                        'turnOverValidBetLastTwoDaysSummary':0,
                        'turnOverValidBetThisMonthSummary':0,
                        'turnOverValidBetLastMonthSummary':0,  
                        
                        'winLossTodaySummary':0,
                        'winLossYesterdaySummary':0,
                        'winLossLastTwoDaysSummary':0,
                        'winLossThisMonthSummary':0,
                        'winLossLastMonthSummary':0
                    });
    
                    let totalItem=totalResult[0];
    
                    currencyList.map(curr=>{
    
                        totalItem[`turnOverValidBetToday_${curr.id}`] = 0;
                        totalItem[`turnOverValidBetYesterday_${curr.id}`] = 0;
                        totalItem[`turnOverValidBetLastTwoDays_${curr.id}`] = 0;
                        totalItem[`turnOverValidBetThisMonth_${curr.id}`] = 0;
                        totalItem[`turnOverValidBetLastMonth_${curr.id}`] = 0;

                        totalItem[`winLossToday_${curr.id}`] = 0;
                        totalItem[`winLossYesterday_${curr.id}`] = 0;
                        totalItem[`winLossLastTwoDays_${curr.id}`] = 0;
                        totalItem[`winLossThisMonth_${curr.id}`] = 0;
                        totalItem[`winLossLastMonth_${curr.id}`] = 0;
    
                    });
    
                    res.data.data.forEach(elem => {

                        //add CategoryList
                        if(!categoryGameList.find(x=>x.categoryId===elem.categoryId)){
                            categoryGameList.push({
                                categoryId:elem.categoryId,
                                categoryName:elem.categoryName,
                                gameList:[]
                            });

                            categoryTotalResult.push({
                                'categoryId': elem.categoryId,
                                'categoryName': elem.categoryName,
                                'gameId': 0, 
                                'gameName': "TOTAL：", 
            
                                'turnOverValidBetTodaySummary':0,
                                'turnOverValidBetYesterdaySummary':0,
                                'turnOverValidBetLastTwoDaysSummary':0,
                                'turnOverValidBetThisMonthSummary':0, 
                                'turnOverValidBetLastMonthSummary':0,

                                'winLossTodaySummary':0,
                                'winLossYesterdaySummary':0,
                                'winLossLastTwoDaysSummary':0,
                                'winLossThisMonthSummary':0, 
                                'winLossLastMonthSummary':0
                                                      
                            });

                            categoryTotalItem=categoryTotalResult.find(x=>x.categoryId===elem.categoryId);

                            currencyList.map(curr=>{
    
                                categoryTotalItem[`turnOverValidBetToday_${curr.id}`] = 0;
                                categoryTotalItem[`turnOverValidBetYesterday_${curr.id}`] = 0;
                                categoryTotalItem[`turnOverValidBetLastTwoDays_${curr.id}`] = 0;
                                categoryTotalItem[`turnOverValidBetThisMonth_${curr.id}`] = 0;
                                categoryTotalItem[`turnOverValidBetLastMonth_${curr.id}`] = 0;

                                categoryTotalItem[`winLossToday_${curr.id}`] = 0;
                                categoryTotalItem[`winLossYesterday_${curr.id}`] = 0;
                                categoryTotalItem[`winLossLastTwoDays_${curr.id}`] = 0;
                                categoryTotalItem[`winLossThisMonth_${curr.id}`] = 0;
                                categoryTotalItem[`winLossLastMonth_${curr.id}`] = 0;
            
                            });
                        }

                        let getCaegoryGameItem=categoryGameList.find(x=>x.categoryId===elem.categoryId);
                        
                        if(getCaegoryGameItem)
                        {
                             //add GameList
                            if(!getCaegoryGameItem.gameList.find(x=>x.gameId===elem.gameId)){
                                getCaegoryGameItem.gameList.push({
                                    gameId:elem.gameId,
                                    gameName:elem.gameName
                                });
                            }
                        }

                        let getItem = result.find(x=>x.categoryId===elem.categoryId && x.gameId===elem.gameId);

                        let turnOverValidBetTodayColDef = `turnOverValidBetToday_${elem.currencyId}`;
                        let turnOverValidBetYesterdayColDef = `turnOverValidBetYesterday_${elem.currencyId}`;
                        let turnOverValidBetLastTwoDaysColDef = `turnOverValidBetLastTwoDays_${elem.currencyId}`;
                        let turnOverValidBetThisMonthColDef = `turnOverValidBetThisMonth_${elem.currencyId}`;
                        let turnOverValidBetLastMonthColDef = `turnOverValidBetLastMonth_${elem.currencyId}`;

                        let winLossTodayColDef = `winLossToday_${elem.currencyId}`;
                        let winLossYesterdayColDef = `winLossYesterday_${elem.currencyId}`;
                        let winLossLastTwoDaysColDef = `winLossLastTwoDays_${elem.currencyId}`;
                        let winLossThisMonthColDef = `winLossThisMonth_${elem.currencyId}`;
                        let winLossLastMonthColDef = `winLossLastMonth_${elem.currencyId}`;

                        if(getItem)
                        {
                            getItem.turnOverValidBetTodaySummary += elem.turnOverValidBetTodaySummaryInMYR;
                            getItem.turnOverValidBetYesterdaySummary += elem.turnOverValidBetYesterdaySummaryInMYR;
                            getItem.turnOverValidBetLastTwoDaysSummary += elem.turnOverValidBetLastTwoDaysSummaryInMYR;
                            getItem.turnOverValidBetThisMonthSummary += elem.turnOverValidBetThisMonthSummaryInMYR;
                            getItem.turnOverValidBetLastMonthSummary += elem.turnOverValidBetLastMonthSummaryInMYR;

                            getItem.winLossTodaySummary += elem.winLossTodaySummaryInMYR;
                            getItem.winLossYesterdaySummary += elem.winLossYesterdaySummaryInMYR;
                            getItem.winLossLastTwoDaysSummary += elem.winLossLastTwoDaysSummaryInMYR;
                            getItem.winLossThisMonthSummary += elem.winLossThisMonthSummaryInMYR;
                            getItem.winLossLastMonthSummary += elem.winLossLastMonthSummaryInMYR;

                            getItem[turnOverValidBetTodayColDef] = elem.turnOverValidBetTodaySummary;
                            getItem[turnOverValidBetYesterdayColDef] = elem.turnOverValidBetYesterdaySummary;
                            getItem[turnOverValidBetLastTwoDaysColDef] = elem.turnOverValidBetLastTwoDaysSummary;
                            getItem[turnOverValidBetThisMonthColDef] = elem.turnOverValidBetThisMonthSummary;
                            getItem[turnOverValidBetLastMonthColDef] = elem.turnOverValidBetLastMonthSummary;

                            getItem[winLossTodayColDef] = elem.winLossTodaySummary;
                            getItem[winLossYesterdayColDef] = elem.winLossYesterdaySummary;
                            getItem[winLossLastTwoDaysColDef] = elem.winLossLastTwoDaysSummary;
                            getItem[winLossThisMonthColDef] = elem.winLossThisMonthSummary;
                            getItem[winLossLastMonthColDef] = elem.winLossLastMonthSummary;
                        }
                        else
                        {
                            result.push({

                                'categoryId':elem.categoryId,
                                'categoryName':elem.categoryName,
                                'gameId':elem.gameId,
                                'gameName':elem.gameName,

                                'turnOverValidBetTodaySummary':elem.turnOverValidBetTodaySummaryInMYR,
                                'turnOverValidBetYesterdaySummary':elem.turnOverValidBetYesterdaySummaryInMYR,
                                'turnOverValidBetLastTwoDaysSummary':elem.turnOverValidBetLastTwoDaysSummaryInMYR,
                                'turnOverValidBetThisMonthSummary':elem.turnOverValidBetThisMonthSummaryInMYR,
                                'turnOverValidBetLastMonthSummary':elem.turnOverValidBetLastMonthSummaryInMYR,

                                'winLossTodaySummary':elem.winLossTodaySummaryInMYR,
                                'winLossYesterdaySummary':elem.winLossYesterdaySummaryInMYR,
                                'winLossLastTwoDaysSummary':elem.winLossLastTwoDaysSummaryInMYR,
                                'winLossThisMonthSummary':elem.winLossThisMonthSummaryInMYR,
                                'winLossLastMonthSummary':elem.winLossLastMonthSummaryInMYR,

                                [turnOverValidBetTodayColDef]:elem.turnOverValidBetTodaySummary,
                                [turnOverValidBetYesterdayColDef]:elem.turnOverValidBetYesterdaySummary,
                                [turnOverValidBetLastTwoDaysColDef]:elem.turnOverValidBetLastTwoDaysSummary,
                                [turnOverValidBetThisMonthColDef]:elem.turnOverValidBetThisMonthSummary,
                                [turnOverValidBetLastMonthColDef]:elem.turnOverValidBetLastMonthSummary,

                                [winLossTodayColDef]:elem.winLossTodaySummary,
                                [winLossYesterdayColDef]:elem.winLossYesterdaySummary,
                                [winLossLastTwoDaysColDef]:elem.winLossLastTwoDaysSummary,
                                [winLossThisMonthColDef]:elem.winLossThisMonthSummary,
                                [winLossLastMonthColDef]:elem.winLossLastMonthSummary

                            });
                        }

                        totalItem.turnOverValidBetTodaySummary += elem.turnOverValidBetTodaySummaryInMYR;
                        totalItem.turnOverValidBetYesterdaySummary += elem.turnOverValidBetYesterdaySummaryInMYR;
                        totalItem.turnOverValidBetLastTwoDaysSummary += elem.turnOverValidBetLastTwoDaysSummaryInMYR;
                        totalItem.turnOverValidBetThisMonthSummary += elem.turnOverValidBetThisMonthSummaryInMYR;
                        totalItem.turnOverValidBetLastMonthSummary += elem.turnOverValidBetLastMonthSummaryInMYR;

                        totalItem.winLossTodaySummary += elem.winLossTodaySummaryInMYR;
                        totalItem.winLossYesterdaySummary += elem.winLossYesterdaySummaryInMYR;
                        totalItem.winLossLastTwoDaysSummary += elem.winLossLastTwoDaysSummaryInMYR;
                        totalItem.winLossThisMonthSummary += elem.winLossThisMonthSummaryInMYR;
                        totalItem.winLossLastMonthSummary += elem.winLossLastMonthSummaryInMYR;

                        totalItem[turnOverValidBetTodayColDef] +=  elem.turnOverValidBetTodaySummary;
                        totalItem[turnOverValidBetYesterdayColDef] += elem.turnOverValidBetYesterdaySummary;
                        totalItem[turnOverValidBetLastTwoDaysColDef] += elem.turnOverValidBetLastTwoDaysSummary;
                        totalItem[turnOverValidBetThisMonthColDef] += elem.turnOverValidBetThisMonthSummary;
                        totalItem[turnOverValidBetLastMonthColDef] += elem.turnOverValidBetLastMonthSummary;

                        totalItem[winLossTodayColDef] += elem.winLossTodaySummary;
                        totalItem[winLossYesterdayColDef] += elem.winLossYesterdaySummary;
                        totalItem[winLossLastTwoDaysColDef] += elem.winLossLastTwoDaysSummary;
                        totalItem[winLossThisMonthColDef] += elem.winLossThisMonthSummary;
                        totalItem[winLossLastMonthColDef] += elem.winLossLastMonthSummary;

                        categoryTotalItem=categoryTotalResult.find(x=>x.categoryId===elem.categoryId);

                        categoryTotalItem.turnOverValidBetTodaySummary += elem.turnOverValidBetTodaySummaryInMYR;
                        categoryTotalItem.turnOverValidBetYesterdaySummary += elem.turnOverValidBetYesterdaySummaryInMYR;
                        categoryTotalItem.turnOverValidBetLastTwoDaysSummary += elem.turnOverValidBetLastTwoDaysSummaryInMYR;
                        categoryTotalItem.turnOverValidBetThisMonthSummary += elem.turnOverValidBetThisMonthSummaryInMYR;
                        categoryTotalItem.turnOverValidBetLastMonthSummary += elem.turnOverValidBetLastMonthSummaryInMYR;

                        categoryTotalItem.winLossTodaySummary += elem.winLossTodaySummaryInMYR;
                        categoryTotalItem.winLossYesterdaySummary += elem.winLossYesterdaySummaryInMYR;
                        categoryTotalItem.winLossLastTwoDaysSummary += elem.winLossLastTwoDaysSummaryInMYR;
                        categoryTotalItem.winLossThisMonthSummary += elem.winLossThisMonthSummaryInMYR;
                        categoryTotalItem.winLossLastMonthSummary += elem.winLossLastMonthSummaryInMYR;

                        categoryTotalItem[turnOverValidBetTodayColDef] += elem.turnOverValidBetTodaySummary;
                        categoryTotalItem[turnOverValidBetYesterdayColDef] += elem.turnOverValidBetYesterdaySummary;
                        categoryTotalItem[turnOverValidBetLastTwoDaysColDef] += elem.turnOverValidBetLastTwoDaysSummary;
                        categoryTotalItem[turnOverValidBetThisMonthColDef] += elem.turnOverValidBetThisMonthSummary;
                        categoryTotalItem[turnOverValidBetLastMonthColDef] += elem.turnOverValidBetLastMonthSummary;

                        categoryTotalItem[winLossTodayColDef] += elem.winLossTodaySummary;
                        categoryTotalItem[winLossYesterdayColDef] += elem.winLossYesterdaySummary;
                        categoryTotalItem[winLossLastTwoDaysColDef] += elem.winLossLastTwoDaysSummary;
                        categoryTotalItem[winLossThisMonthColDef] += elem.winLossThisMonthSummary;
                        categoryTotalItem[winLossLastMonthColDef] += elem.winLossLastMonthSummary;

                    });
                }

                setData({
                    ...stateData,
                    Datas:[
                        ...result.map(item=>{
                            Object.keys(item).forEach(key => {
                                if(key.startsWith('turnOverValidBet') || 
                                   key.startsWith('winLoss'))
                                {
                                    item[key] = number_format(number_toFixed(item[key], 2));
                                }
                            });

                            return {...item}
                        })
                    ],
                    TotalResult: [
                        ...totalResult.map(item=>{
                        Object.keys(item).forEach(key => {
                            if(key.startsWith('turnOverValidBet') || 
                               key.startsWith('winLoss'))
                            {
                                item[key] = number_format(number_toFixed(item[key], 2));
                            }
                            });

                            return {...item}
                        })
                    ],
                    CategoryTotalResult: [
                        ...categoryTotalResult.map(item=>{
                        Object.keys(item).forEach(key => {
                            if(key.startsWith('turnOverValidBet') || 
                               key.startsWith('winLoss'))
                            {
                                item[key] = number_format(number_toFixed(item[key], 2));
                            }
                            });

                            return {...item}
                        })
                    ],
                    CurrencyList:[...currencyList],
                    CategoryGameList:[...categoryGameList]
                });
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        });
    }

    const ExportToExcel = ()=>{

        if (stateFilter.SiteIds.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }

        setSubmitStatus(true);
        setExportStatus(true);

        let param = {
            Url: 'Report/ExportStatisticalReport',
            Data:{
                SiteIds: JSON.stringify(stateFilter.SiteIds),
                EndDate: moment().format('YYYY-MM-DD')
            }
        };

        WebAPI.DownloadData(param);

        setTimeout(() => {
            setSubmitStatus(false);
            setExportStatus(false);
        }, 5000);
    }

    return (
        <div style={{ width: "100%", height: "100%", padding: "15px" }}>
             <h4 className="page-name">Statistical Report</h4>
            <Filter filterInfo={stateFilter} 
                    SiteList={siteList}
                    onChange={Filter_OnChange}
                    onChangeCheckBox={Filter_CheckBox_OnChange} 
                    onChangeAllCheckBox={Filter_CheckBox_CheckAll}  
                    ClickSubmit={Filter_Submit}
                    SubmitBtnDisabled={SubmitStatus} />
            {stateData.Datas === null 
            ? null 
            : <Result {...stateData}
                ExportToExcel={ExportToExcel}
                ExportBtnDisabled={ExportStatus}
                HidenTitles={HiddenTitles}
                TurnOverValidBetHideShowBtn={Result_ValidBetHideShowBtnOnClick}
                WinLossHideShowBtn={Result_WinLossHideShowBtnOnClick} />}
        </div>
    );
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}