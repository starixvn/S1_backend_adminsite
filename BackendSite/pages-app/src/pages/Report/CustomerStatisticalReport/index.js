import React, {useState, useRef, useEffect} from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import moment from 'moment';

const App = ()=>{

    const [stateFilter, setFilter] = useState({
        'SiteIds': [],
        'Currency': [],
        'StartDate': moment().add(-7, 'days').format('l'),
        'EndDate': moment().format('l'),
        'UserName':null
    });

    const [stateData, setData] = useState({
        FilterInfo: null,
        Datas: null,
        DatasByDate: null,
        UserCategoryTotalResult: null,
        UserTotalResult: null,
        TotalResult: null,
        CurrencyList: null,
        UserList: null,
        //for date detail
        DateDetailDatas:null
    });

    const [siteList, setSiteList] = useState(); 
    const [SubmitStatus, setSubmitStatus] = useState(false);
    const [ExportStatus, setExportStatus] = useState(false);

    const [UserDataShowLists, setUserDataShowLists] = useState({});
    const [UserDataShowStatus, setUserDataShowStatus] = useState(false);
    const [UserCategoryDataShowLists, setUserCategoryDataShowLists] = useState({});
    const [UserCategoryDataShowStatus, setUserCategoryDataShowStatus] = useState(false);
    const [UserGameDataShowLists, setUserGameDataShowLists] = useState({});
    const [UserGameDataShowStatus, setUserGameDataShowStatus] = useState(false);
    const [UserDateDataShowLists, setUserDateDataShowLists] = useState({});
    const [UserDateDataShowStatus, setUserDateDataShowStatus] = useState(false);

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

    //#region Filter Component 

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

    //#endregion

    //#region  Result Component

    const Result_UserShowBtnOnClick = (custid) => {

        let getUserItem=stateData.UserList.find(x=>x.custId===custid);

        setUserDataShowLists({
                'custId': getUserItem.custId,
                'userName':getUserItem.userName,
                'categoryList':getUserItem.categoryList,
                'currencyList':getUserItem.currencyList
        });

        setUserDataShowStatus(true);

        setUserCategoryDataShowStatus(false);
        setUserCategoryDataShowLists({});
        setUserGameDataShowStatus(false);
        setUserGameDataShowLists({});
        setUserDateDataShowStatus(false);
        setUserDateDataShowLists({});
    }

    const Result_UserCategoryShowBtnOnClick = (custid,categoryid) => {

        let getUserItem=stateData.UserList.find(x=>x.custId===custid);
        let getCategoryItem = getUserItem.categoryList.find(x=>x.categoryId===categoryid);
        
        setUserCategoryDataShowLists({
                'custId': getUserItem.custId,
                'userName':getUserItem.userName,
                'categoryId':getCategoryItem.categoryId,
                'categoryName':getCategoryItem.categoryName,
                'gameList':getCategoryItem.gameList,
                'currencyList':getUserItem.currencyList
        });

        setUserCategoryDataShowStatus(true);

        setUserDataShowStatus(false);
        setUserDataShowLists({});
        setUserGameDataShowStatus(false);
        setUserGameDataShowLists({});
        setUserDateDataShowStatus(false);
        setUserDateDataShowLists({});
    }

    const Result_UserGameShowBtnOnClick = (custid,categoryid,gameid) => {

        let getUserItem=stateData.UserList.find(x=>x.custId===custid);
        let getCategoryItem = getUserItem.categoryList.find(x=>x.categoryId===categoryid);
        let getGameItem = getCategoryItem.gameList.find(x=>x.gameId===gameid);

        setUserGameDataShowLists({
                'custId': getUserItem.custId,
                'userName':getUserItem.userName,
                'categoryId':getCategoryItem.categoryId,
                'categoryName':getCategoryItem.categoryName,
                'gameId':getGameItem.gameId,
                'gameName':getGameItem.gameName,
                'currencyList':getUserItem.currencyList
        });

        setUserGameDataShowStatus(true);

        setUserDataShowStatus(false);
        setUserDataShowLists({});
        setUserCategoryDataShowStatus(false);
        setUserCategoryDataShowLists({});
        setUserDateDataShowStatus(false);
        setUserDateDataShowLists({});
    }

    const Result_UserDateShowBtnOnClick = (custid,categoryid,gameid,date) => {

        let param = {
            Url: 'Report/GetCustomerDetail',
            Data: {
                CustId: custid,
                GameId: gameid,
                Date: moment(date).format('YYYY-MM-DD')
            }
        };

        WebAPI.PostData(param).then((res) => {

            if (res.data.errorCode === 0) {

                setData({
                    FilterInfo: stateData.FilterInfo,
                    Datas: stateData.Datas,
                    DatasByDate: stateData.DatasByDate,
                    UserCategoryTotalResult: stateData.UserCategoryTotalResult,
                    UserTotalResult: stateData.UserTotalResult,
                    TotalResult: stateData.TotalResult,
                    CurrencyList: stateData.CurrencyList,
                    UserList: stateData.UserList,
                    DateDetailDatas: [...res.data.data]
                });

                //setUserGameDataShowLists({});
                //setUserGameDataShowStatus(false);

                let getUserItem=stateData.UserList.find(x=>x.custId===custid);
                let getCategoryItem = getUserItem.categoryList.find(x=>x.categoryId===categoryid);
                let getGameItem = getCategoryItem.gameList.find(x=>x.gameId===gameid);

                setUserDateDataShowLists({
                    'custId': getUserItem.custId,
                    'userName':getUserItem.userName,
                    'categoryId':getCategoryItem.categoryId,
                    'categoryName':getCategoryItem.categoryName,
                    'gameId':getGameItem.gameId,
                    'gameName':getGameItem.gameName,
                    'currencyList':getUserItem.currencyList,
                    'date':date
                });

                setUserDateDataShowStatus(true);

                setUserDataShowStatus(false);
                setUserDataShowLists({});
                setUserCategoryDataShowStatus(false);
                setUserCategoryDataShowLists({});
                setUserGameDataShowStatus(false);
                setUserGameDataShowLists({});

            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        });

    }

    //All Poppout Close
    const Result_CloseBtnClick = () => {

            //All Poppout Close
            setUserDataShowStatus(false);
            setUserDataShowLists({});
            setUserCategoryDataShowStatus(false);
            setUserCategoryDataShowLists({});
            setUserGameDataShowStatus(false);
            setUserGameDataShowLists({});
            setUserDateDataShowStatus(false);
            setUserDateDataShowLists({});

    }
    
    //#endregion

    //#region Main Methods

    const Filter_Submit = ()=>{

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
            Url: 'Report/GetCustomerStatisticalReport',
            Data: {
                SiteIds: stateFilter.SiteIds,
                CurrencyIds: currencyIds,
                StartDate: moment(stateFilter.StartDate).format('YYYY-MM-DD'),
                EndDate: moment(stateFilter.EndDate).format('YYYY-MM-DD'),
                UserName: stateFilter.UserName
            }
        };

        setData({ 
            FilterInfo: [],
            Datas: null,
            DatasByDate: null,
            UserCategoryTotalResult: null,
            UserTotalResult: null,
            TotalResult: null,
            CurrencyList: null,
            UserList: null
        });

        WebAPI.PostData(param).then((res) => {

            setSubmitStatus(false);

            if (res.data.errorCode === 0) {

                let totalResult = [];
                let result = [];
                let userList = [];
                let currencyList = [];
                let userCategoryTotalResult=[];
                let userTotalResult=[];
                let userDateList = [];

                if(res.data.data.length!==0){

                    res.data.data.forEach(elem => {

                        //#region Add CurrencyList
    
                        if(!currencyList.find(x=>x.id===elem.currencyId)){
                            currencyList.push({
                                id:elem.currencyId,
                                code:elem.isoCode
                            });
                        }
    
                        //#endregion
    
                        //#region UserList:add User Data
    
                        if(!userList.find(x=>x.custId===elem.custId)){
                               
                            userList.push({
                                custId:elem.custId,
                                userName:elem.userName,
                                gameCount:0,
                                categoryCount:0,
                                categoryList:[],
                                currencyList:[]
                            });
                        }
    
                        //#endregion
    
                        //#region UserList:add User Category Data
    
                        let getUserItem=userList.find(x=>x.custId===elem.custId);
    
                        if(!getUserItem.categoryList.find(x=>x.categoryId===elem.categoryId) && getUserItem.custId === elem.custId)
                        {
                            getUserItem.categoryCount += 1;
    
                            getUserItem.categoryList.push({
                                'categoryId':elem.categoryId,
                                'categoryName':elem.categoryName,
                                'gameList':[]
                            });
                        }
    
                        //#endregion

                        //#region UserList:add User Currency Data
    
                        if(!getUserItem.currencyList.find(x=>x.id===elem.currencyId) && getUserItem.custId === elem.custId)
                        {
                            getUserItem.currencyList.push({
                                'id':elem.currencyId,
                                'code':elem.isoCode
                            });
                        }

                        //#endregion

                        //#region UserList:add User Category Game Data
    
                        let getUserCaegoryItem=getUserItem.categoryList.find(x=>x.categoryId===elem.categoryId);
    
                        if(!getUserCaegoryItem.gameList.find(x=>x.gameId===elem.gameId && getUserItem.custId === elem.custId)){
    
                            getUserItem.gameCount += 1;
    
                            getUserCaegoryItem.gameList.push({
                                    gameId:elem.gameId,
                                    gameName:elem.gameName
                            });
                        }
    
                        //#endregion

                    });
                        
                    currencyList = currencyList.sort(function (a, b) {
                        return a.id > b.id ? 1 : -1;
                    });

                    res.data.data.forEach(elem => {

                        //#region UserCategoryTotalResult Initialize

                        if(!userCategoryTotalResult.find(x=>x.custId===elem.custId && x.categoryId===elem.categoryId))
                        {

                            userCategoryTotalResult.push({
                                'custId': elem.custId,
                                'userName': elem.userName,
                                'categoryId': elem.categoryId,
                                'categoryName': elem.categoryName,
                                'gameId': 0, 
                                'gameName': "TOTAL：", 
            
                                'turnOverValidBetSummary':0,
                                'winLossSummary':0,
                                                      
                            });

                            let categoryTotalItem=userCategoryTotalResult.find(x=>x.custId===elem.custId && x.categoryId===elem.categoryId);

                            currencyList.map(curr=>{
                                categoryTotalItem[`turnOverValidBet_${curr.id}`] = 0;
                                categoryTotalItem[`winLoss_${curr.id}`] = 0;
            
                            });
                        }

                        //#endregion

                        //#region UserTotalResult Initialize

                        if(!userTotalResult.find(x=>x.custId===elem.custId))
                        {
                            userTotalResult.push({
                                'custId': elem.custId,
                                'userName': elem.userName,
                                'categoryId': 0,
                                'categoryName': "USER",
                                'gameId': 0, 
                                'gameName': "TOTAL：", 
            
                                'turnOverValidBetSummary':0,
                                'winLossSummary':0,
                                                      
                            });

                            let userTotalItem=userTotalResult.find(x=>x.custId===elem.custId);

                            currencyList.map(curr=>{
                                userTotalItem[`turnOverValidBet_${curr.id}`] = 0;
                                userTotalItem[`winLoss_${curr.id}`] = 0;
                            });
                        }

                        //#endregion
                    
                        //#region UserDateList Setting

                        let getItem=userDateList.find(x=>x.date===elem.date && x.custId===elem.custId && x.categoryId===elem.categoryId && x.gameId===elem.gameId);

                        let turnOverValidBetColDef = `turnOverValidBet_${elem.currencyId}`;
                        let winLossColDef = `winLoss_${elem.currencyId}`;

                        if(!getItem)
                        {
                            userDateList.push({
                                'date':elem.date,
                                'custId':elem.custId,
                                'userName':elem.userName,
                                'categoryId':elem.categoryId,
                                'categoryName':elem.categoryName,
                                'gameId':elem.gameId,
                                'gameName':elem.gameName,

                                'turnOverValidBetSummary':elem.turnOverValidBetSummaryInMYR,
                                'winLossSummary':elem.winLossSummaryInMYR,
                                [turnOverValidBetColDef]:elem.turnOverValidBetSummary,
                                [winLossColDef]:elem.winLossSummary
                            });
                        }
                        else
                        {
                            getItem.turnOverValidBetSummary += elem.turnOverValidBetSummaryInMYR;
                            getItem.winLossSummary += elem.winLossSummaryInMYR;

                            getItem[turnOverValidBetColDef] += elem.turnOverValidBetSummary;
                            getItem[winLossColDef] += elem.winLossSummary;
                        }

                        //#endregion

                    });

                    //#region TotalResult Initialize

                    totalResult.push({
                        'userName':"",
                        'categoryName':"",
                        'gameName':"TOTAL：",
    
                        'turnOverValidBetSummary':0,
                        'winLossSummary':0
                    });
    
                    let totalItem=totalResult[0];
    
                    currencyList.map(curr=>{
                        totalItem[`turnOverValidBet_${curr.id}`] = 0;
                        totalItem[`winLoss_${curr.id}`] = 0;
                    });

                    //#endregion
    
                    res.data.data.forEach(elem => {

                        let getItem = result.find(x=>x.custId===elem.custId && x.categoryId===elem.categoryId && x.gameId===elem.gameId);

                        let turnOverValidBetColDef = `turnOverValidBet_${elem.currencyId}`;
                        let winLossColDef = `winLoss_${elem.currencyId}`;

                        if(getItem)
                        {
                            getItem.turnOverValidBetSummary += elem.turnOverValidBetSummaryInMYR;
                            getItem.winLossSummary += elem.winLossSummaryInMYR;

                            getItem[turnOverValidBetColDef] += elem.turnOverValidBetSummary;
                            getItem[winLossColDef] += elem.winLossSummary;
                        }
                        else
                        {
                            result.push({
                                'custId':elem.custId,
                                'userName':elem.userName,
                                'categoryId':elem.categoryId,
                                'categoryName':elem.categoryName,
                                'gameId':elem.gameId,
                                'gameName':elem.gameName,

                                'turnOverValidBetSummary':elem.turnOverValidBetSummaryInMYR,
                                'winLossSummary':elem.winLossSummaryInMYR,
                                [turnOverValidBetColDef]:elem.turnOverValidBetSummary,
                                [winLossColDef]:elem.winLossSummary
                            });
                        }
                        
                        //#region UserCategoryTotalResult Setting

                        let getUserCategoryTotalItem=userCategoryTotalResult.find(x=>x.custId===elem.custId && x.categoryId===elem.categoryId);

                        getUserCategoryTotalItem.turnOverValidBetSummary += elem.turnOverValidBetSummaryInMYR;
                        getUserCategoryTotalItem.winLossSummary += elem.winLossSummaryInMYR;

                        getUserCategoryTotalItem[turnOverValidBetColDef] += elem.turnOverValidBetSummary;
                        getUserCategoryTotalItem[winLossColDef] += elem.winLossSummary;

                        //#endregion

                        //#region UserCategoryTotalResult Setting

                        let getUserTotalItem=userTotalResult.find(x=>x.custId===elem.custId);

                        getUserTotalItem.turnOverValidBetSummary += elem.turnOverValidBetSummaryInMYR;
                        getUserTotalItem.winLossSummary += elem.winLossSummaryInMYR;
                  
                        getUserTotalItem[turnOverValidBetColDef] += elem.turnOverValidBetSummary;
                        getUserTotalItem[winLossColDef] += elem.winLossSummary;
                  
                        //#endregion

                        //#region TotalResult Setting 

                        totalItem.turnOverValidBetSummary += elem.turnOverValidBetSummaryInMYR;
                        totalItem.winLossSummary += elem.winLossSummaryInMYR;

                        totalItem[turnOverValidBetColDef] +=  elem.turnOverValidBetSummary;
                        totalItem[winLossColDef] += elem.winLossSummary;

                        //#endregion

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
                                    item[key] = numberWithCommas(item[key].toFixed(2));
                                }
                            });

                            return {...item}
                        })
                    ],
                    DatasByDate:[
                        ...userDateList.map(item=>{
                            Object.keys(item).forEach(key => {
                                if(key.startsWith('turnOverValidBet') || 
                                   key.startsWith('winLoss'))
                                {
                                    item[key] = numberWithCommas(item[key].toFixed(2));
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
                                item[key] = numberWithCommas(item[key].toFixed(2));
                            }
                            });

                            return {...item}
                        })
                    ],
                    UserCategoryTotalResult: [
                        ...userCategoryTotalResult.map(item=>{
                        Object.keys(item).forEach(key => {
                            if(key.startsWith('turnOverValidBet') || 
                               key.startsWith('winLoss'))
                            {
                                item[key] = numberWithCommas(item[key].toFixed(2));
                            }
                            });

                            return {...item}
                        })
                    ],
                    UserTotalResult: [
                        ...userTotalResult.map(item=>{
                        Object.keys(item).forEach(key => {
                            if(key.startsWith('turnOverValidBet') || 
                               key.startsWith('winLoss'))
                            {
                                item[key] = numberWithCommas(item[key].toFixed(2));
                            }
                            });

                            return {...item}
                        })
                    ],
                    CurrencyList:[...currencyList],
                    UserList:[...userList],
                    FilterInfo: {
                        ...stateFilter
                    },
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
            Url: 'Report/ExportCustomerStatisticalReport',
            Data:{
                SiteIds: JSON.stringify(stateData.FilterInfo.SiteIds),
                CurrencyIds: JSON.stringify(currencyIds),
                StartDate: moment(stateData.FilterInfo.StartDate).format('YYYY-MM-DD'),
                EndDate: moment(stateData.FilterInfo.EndDate).format('YYYY-MM-DD'),
                UserName: stateData.FilterInfo.UserName
            }
        };

        WebAPI.DownloadData(param);

        setTimeout(() => {
            setSubmitStatus(false);
            setExportStatus(false);
        }, 5000);
    }

    //#endregion

    return (
        <div style={{ width: "100%", height: "100%", padding: "15px" }}>
             <h4 className="page-name">Customer Statistical Report</h4>
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
                UserDataShowLists={UserDataShowLists}
                UserDataShowStatus={UserDataShowStatus}
                UserCategoryDataShowLists={UserCategoryDataShowLists}
                UserCategoryDataShowStatus={UserCategoryDataShowStatus}
                UserGameDataShowLists={UserGameDataShowLists}
                UserGameDataShowStatus={UserGameDataShowStatus}
                UserDateDataShowLists={UserDateDataShowLists}
                UserDateDataShowStatus={UserDateDataShowStatus}
                CloseBtn={Result_CloseBtnClick}
                SetUserShowBtn={Result_UserShowBtnOnClick}
                SetUserCategoryShowBtn={Result_UserCategoryShowBtnOnClick}
                SetUserGameShowBtn={Result_UserGameShowBtnOnClick}
                SetUserDateShowBtn={Result_UserDateShowBtnOnClick}/>}
        </div>
    );
}

const numberWithCommas = (n)=> {
    n += "";
    var arr = n.split(".");
    var re = /(\d{1,3})(?=(\d{3})+$)/g;
    return arr[0].replace(re, "$1,") + (arr.length == 2 ? "." + arr[1] : "");
};

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}