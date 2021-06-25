import Filter from "./Filter";
import Result from "./Result";
import { getUrlParameter } from '../../commJS/common';

const Panel = (props) => {
    const [TagList, setTagList] = React.useState(null);
    const [NewstateFilter, setNewFilter] = React.useState({
        SelectTag: []
    });
    const [searchData,setsearchData] = React.useState(null)
    let CustId = getUrlParameter("custId");
    let iCustId = parseInt(CustId);

    React.useEffect(() => {
        GetCustInfoDetail();
        axios.post('/api/CustManage/GetAllTag')
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        setTagList(result.data)
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
    }, []);

    const GetCustInfoDetail=()=>{
        axios.post('/api/CustManage/GetCustData',{CustId: iCustId})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        let taglist = [];
                        console.log(result.data.iTagList);
                        for (const child of result.data.iTagList) {
                            taglist.push(child);
                        }
                        setNewFilter({
                            SelectTag:taglist
                        });
                        setsearchData(result.data)
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
    }
    
    const Filter_CheckBox_OnChange = (v, dataColumn) => {
        console.log(searchData,1)
        let isRemove = NewstateFilter[dataColumn].includes(v);
        let newChecked = isRemove
            ? NewstateFilter[dataColumn].filter((value) => value !== v)
            : NewstateFilter[dataColumn].push(v);
        let currentTab = NewstateFilter[dataColumn];
        if (newChecked.length === 0) {
            currentTab = [];
        } else if (currentTab === null) {
            currentTab = newChecked;
        }

        if (isRemove && !newChecked.includes(currentTab)) {
            currentTab = newChecked.length === 0 ? [] : newChecked;
        }
        setNewFilter({
            [dataColumn]: currentTab,
        });
        console.log(searchData,2)
    };

    const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
        if (v !== "All") return false;
        let currentTab =
        NewstateFilter[dataColumn].length === options.length
            ? []
            : options.map((o) => {
                return o.value;
                });
        setNewFilter({
            ...NewstateFilter,
            [dataColumn]: currentTab,
        });
    };

    const Filter_Submit = () => {
        axios.post('/api/CustManage/SetCustTag',{CustId: iCustId,SelectTag:NewstateFilter.SelectTag})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        GetCustInfoDetail();
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
    }

    const RemoveTag=()=>{
        axios.post('/api/CustManage/SetCustTag',{CustId: iCustId,SelectTag:[]})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        GetCustInfoDetail();
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
    }

    return (
        <>
            {searchData != null && TagList != null? (<Filter
                    TagList={TagList}
                    FilterData={NewstateFilter}
                    onChangeCheckBox={Filter_CheckBox_OnChange}
                    onChangeAllCheckBox={Filter_CheckBox_CheckAll}
                    ClickSubmit={Filter_Submit}
                    RemoveTag={RemoveTag}
                />):null} 
            {searchData != null && TagList != null? (<Result
                    TagList={TagList}
                    Data={searchData.iTagList}
                />):null} 
        </>
    )
}
export default Panel;