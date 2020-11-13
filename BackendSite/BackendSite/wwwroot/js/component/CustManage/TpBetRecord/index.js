import { getUrlParameter } from '../../commJS/common';
import WeekResult from './WeekResult';
import DayResult from './DayResult';
import CategoryResult from './CategoryResult';
import GameResult from './GameResult';

const Panel = () => {
    const [stateWeekDatas, setWeekDatas] = React.useState();
    const [stateDayDatas, setDayDatas] = React.useState();
    const [stateCategoryDatas, setCategoryDatas] = React.useState();
    const [stateGameDatas, setGameDatas] = React.useState();
    const [stateGameInfo, setGameInfo] = React.useState({ Date: null, GameTypeId: null });
    const [stateInfoType, setInfoType] = React.useState();
    let CustId = parseInt(getUrlParameter("custId"));

    React.useEffect(() => {
        axios.post('/api/Payment/GetCustTpBetRecordListOneWeek', { CustId: CustId })
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    setWeekDatas(result.data);
                    setInfoType("week");
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            }.bind(this));
    }, []);

    const getDayData = (Date) => {
        axios.post('/api/Payment/GetCustTpBetRecordListOneDay', { CustId: CustId, selDate: moment(Date).unix() })
            .then((res) => {
                let result = res.data;
                if (result.errorCode == 0) {
                    setDayDatas(result.data);
                    setInfoType("day");
                } else {
                    alert(result.message);
                }
            }).catch((error) => {
                alert(error);
            });
    }

    const getCategoryData = (Date, CategoryId) => {
        axios.post('/api/Payment/GetCustTpBetRecordListbyCategoryId', { CustId: CustId, selDate: moment(Date).unix(), CategoryId: parseInt(CategoryId) })
            .then((res) => {
                let result = res.data;
                if (result.errorCode == 0) {
                    setCategoryDatas(result.data);
                    setInfoType("category");
                } else {
                    alert(result.message);
                }
            }).catch((error) => {
                alert(error);
            });
    }

    const getGameData = (Date, GameTypeId, PageNumber) => {
        axios.post('/api/Payment/GetCustTpBetRecordDetailListbyGameType',
            {
                CustId: CustId,
                selDate: moment(Date).unix(),
                gameTypeId: parseInt(GameTypeId),
                PageSize: 15,
                PageNumber: PageNumber
            })
            .then((res) => {
                let result = res.data;
                if (result.errorCode == 0) {
                    setGameDatas(result);
                    setGameInfo({ Date: Date, GameTypeId: GameTypeId })
                    setInfoType("game");
                } else {
                    alert(result.message);
                }
            }).catch((error) => {
                alert(error);
            });
    }

    const onClickBack = (type) => {
        setInfoType(type);
    }

    return (
        stateInfoType === "week" ?
            <WeekResult Datas={stateWeekDatas} ClickBack={onClickBack} ClickDetail={getDayData} /> : stateInfoType === "day" ?
                <DayResult Datas={stateDayDatas} ClickBack={onClickBack} ClickDetail={getCategoryData} /> : stateInfoType === "category" ?
                    <CategoryResult Datas={stateCategoryDatas} ClickBack={onClickBack} ClickDetail={getGameData} /> : stateInfoType === "game" ?
                        <GameResult Datas={stateGameDatas} Info={stateGameInfo} ClickBack={onClickBack} ClickDetail={getGameData} /> : null
    )
}

export default Panel;