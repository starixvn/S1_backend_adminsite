import { number_format } from '../../commJS/common';
import moment from "moment-timezone";
const Log = (props) => {

    let levelSettingArr = {};
    if (props.Setting) {
        Object.values(props.Setting).map(i => {
            levelSettingArr[i.level] = i.levelName;
        })
    }

    let rows = props.Logs.length != 0 ? props.Logs.map((row, idx) => {
        return (
            <tr key={"data-" + idx}>
                <th>{levelSettingArr[number_format(row.oldLevel)]}</th>
                <th>{levelSettingArr[number_format(row.newLevel)]}</th>
                <th>{row.memo}</th>
                <td>{moment(row.updateTime).format('l LTS')}</td>
                <th>{row.updaters}</th>
            </tr>
        );
    }) : null;

    return (
        <div className="table" >
            <table className="table table-responsive-md">
                <thead>
                    <tr>
                        <th scope="col">Old Level</th>
                        <th scope="col">New Level</th>
                        <th scope="col">Memo</th>
                        <th scope="col">Operating time(Adjusted to GMT+8)</th>
                        <th scope="col">Operator</th>
                    </tr>
                </thead>
                <tbody>
                    {rows}
                </tbody>
            </table>
        </div>
    )
}

export default Log;